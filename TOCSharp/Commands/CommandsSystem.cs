using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using TOCSharp.Commands.Attributes;
using TOCSharp.Commands.Converters;
using TOCSharp.Models;

namespace TOCSharp.Commands
{
    public class CommandsSystem
    {
        public CommandsSystemSettings Settings { get; }

        internal Dictionary<Type, IArgumentConverter> ArgumentConverters { get; } = new Dictionary<Type, IArgumentConverter>()
        {
            [typeof(string)] = new StringConverter(),
            [typeof(int)] = new IntConverter(),
            [typeof(uint)] = new UIntConverter(),
            [typeof(long)] = new LongConverter(),
            [typeof(ulong)] = new ULongConverter(),
            [typeof(short)] = new ShortConverter(),
            [typeof(ushort)] = new UShortConverter(),
            [typeof(byte)] = new ByteConverter(),
            [typeof(sbyte)] = new SByteConverter(),
            [typeof(bool)] = new BoolConverter(),
            [typeof(float)] = new FloatConverter(),
            [typeof(double)] = new DoubleConverter(),
            [typeof(decimal)] = new DecimalConverter(),
        };

        public Dictionary<string, CommandInfo> Commands { get; } = new Dictionary<string, CommandInfo>();

        public TOCClient Client { get; }

        private readonly MethodInfo ConvertGeneric;

        internal CommandsSystem(TOCClient client, CommandsSystemSettings? settings)
        {
            this.Client = client;
            settings ??= new CommandsSystemSettings();
            this.Settings = settings;

            Type ncvt = typeof(NullableConverter<>);
            Type nt = typeof(Nullable<>);
            Type[] cvts = this.ArgumentConverters.Keys.ToArray();
            foreach (Type? xt in cvts)
            {
                TypeInfo xti = xt.GetTypeInfo();
                if (!xti.IsValueType)
                {
                    continue;
                }

                Type xcvt = ncvt.MakeGenericType(xt);
                Type xnt = nt.MakeGenericType(xt);

                if (this.ArgumentConverters.ContainsKey(xcvt) || !(Activator.CreateInstance(xcvt) is IArgumentConverter xcv))
                {
                    continue;
                }

                this.ArgumentConverters[xnt] = xcv;
            }

            Type t = typeof(CommandsSystem);
            IEnumerable<MethodInfo> ms = t.GetTypeInfo().DeclaredMethods;
            MethodInfo? m = null;
            foreach (MethodInfo xm in ms)
            {
                if (xm is { Name: nameof(ConvertArgument), ContainsGenericParameters: true } && xm is { IsStatic: false, IsAssembly: true })
                {
                    m = xm;
                    break;
                }
            }
            this.ConvertGeneric = m!;

            if (this.Settings.UseDefaultHelpCommand)
            {
                this.RegisterCommands(new DefaultHelpCommand());
            }

            this.Client.ChatMessageReceived += this.ChatMessageReceived;
            this.Client.IMReceived += ClientOnIMReceived;
        }

        public void RegisterCommands(ICommandModule commandModule)
        {
            foreach (MethodInfo method in commandModule.GetType().GetMethods())
            {
                CommandAttribute? cmd = method.GetCustomAttribute<CommandAttribute>();
                if (cmd == null) continue;

                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 0) continue;
                if (parameters[0].ParameterType != typeof(CommandContext)) continue;

                ParameterInfo[] paramInfos = Array.Empty<ParameterInfo>();
                int minParamLen = 0;
                if (parameters.Length != 1)
                {
                    paramInfos = parameters[1..];
                    minParamLen = parameters[1..].Count(x => !x.HasDefaultValue);
                }

                CommandInfo info = new CommandInfo
                {
                    Instance = commandModule,
                    Method = method,
                    Attribute = cmd,
                    TypeParams = paramInfos,
                    MinParamLen = minParamLen
                };

                //Console.WriteLine("Registered command: " + string.Join(", ", cmd.Names) + "; ParamInfos: " + string.Join(", ", paramInfos.Select(x => x.ParameterType.FullName)));

                foreach (string name in cmd.Names)
                {
                    string n = name;
                    if (!this.Settings.CaseSensitive)
                    {
                        n = name.ToLower();
                    }
                    this.Commands[n] = info;
                }
            }
        }

        private async Task ClientOnIMReceived(object sender, InstantMessage args)
        {
            if (Utils.NormalizeScreenname(args.Sender.Screenname) == Utils.NormalizeScreenname(this.Client.Screenname)) return;
            string msg = Utils.StripHTML(args.Message);

            CommandContext ctx = new CommandContext()
            {
                IsChat = false,
                IsWhisper = false,
                CommandsSystem = this,
                Message = msg,
                Sender = args.Sender
            };

            await this.RouteCommand(ctx);
        }

        private async Task ChatMessageReceived(object sender, ChatMessage args)
        {
            if (Utils.NormalizeScreenname(args.Sender.Screenname) == Utils.NormalizeScreenname(this.Client.Screenname)) return;
            string msg = Utils.StripHTML(args.Message);

            CommandContext ctx = new CommandContext()
            {
                IsChat = true,
                IsWhisper = args.Whisper,
                ChatRoom = args.RoomID,
                CommandsSystem = this,
                Message = msg,
                Sender = args.Sender
            };

            await this.RouteCommand(ctx);
        }

        private async Task RouteCommand(CommandContext ctx)
        {
            ctx.Message = ctx.Message.Replace("&quot;", "\"");

            string? raw = null;
            foreach (string? prefix in this.Settings.StringPrefixes)
            {
                if (ctx.Message.StartsWith(prefix))
                {
                    ctx.Prefix = prefix;
                    raw = ctx.Message[prefix.Length..];
                    break;
                }
            }

            if (raw == null) return;
            int pos = 0;
            string? command = raw.ExtractNextArgument(ref pos, this.Settings.QuotationMarks);

            if (command == null) return;

            if (!this.Settings.CaseSensitive)
            {
                command = command.ToLower();
            }

            if (!this.Commands.TryGetValue(command, out CommandInfo? info)) return;

            List<object?> arguments = new List<object?> { ctx };

            foreach (ParameterInfo param in info.TypeParams)
            {
                RemainingTextAttribute? remaining = param.GetCustomAttribute<RemainingTextAttribute>();

                string? arg = null;
                if (remaining != null)
                {
                    arg = pos >= raw.Length ? null : raw[(pos + 1)..];
                }
                else
                {
                    arg = raw.ExtractNextArgument(ref pos, this.Settings.QuotationMarks);
                }

                if (arg == null)
                {
                    if (!param.HasDefaultValue)
                        return;

                    arguments.Add(param.DefaultValue);
                    continue;
                }

                if (!this.ArgumentConverters.TryGetValue(param.ParameterType, out IArgumentConverter? converter))
                {
                    return;
                }

                try
                {
                    object? val = await this.ConvertArgument(arg, ctx, param.ParameterType);
                    arguments.Add(val);
                }
                catch (Exception)
                {
                    return;
                }
            }

            try
            {
                info.Method.Invoke(info.Instance, arguments.ToArray());
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to invoke command " + command + ": " + e);
                return;
            }
        }

        internal async Task<object?> ConvertArgument(string? value, CommandContext ctx, Type type)
        {
            MethodInfo m = this.ConvertGeneric.MakeGenericMethod(type);
            try
            {
                return await (Task<object?>)m.Invoke(this, new object[] { value!, ctx })!;
            }
            catch (Exception ex)
            {
                throw ex.InnerException!;
            }
        }

        internal async Task<object?> ConvertArgument<T>(string value, CommandContext ctx)
        {
            Type t = typeof(T);
            if (!this.ArgumentConverters.TryGetValue(t, out IArgumentConverter? conv))
            {
                throw new ArgumentException("There is no converter specified for given type.", nameof(T));
            }

            if (!(conv is IArgumentConverter<T> cv))
            {
                throw new ArgumentException("Invalid converter registered for this type.", nameof(T));
            }

            T cvr = await cv.ConvertAsync(ctx, value);
            return cvr == null ? throw new ArgumentException("Could not convert specified value to given type.", nameof(value)) : cvr;
        }
    }

    public class CommandInfo
    {
        public ICommandModule Instance { get; set; } = null!;
        public MethodInfo Method { get; set; } = null!;
        public CommandAttribute Attribute { get; set; } = null!;
        public ParameterInfo[] TypeParams { get; set; } = null!;
        public int MinParamLen { get; set; }
    }

    public static class CommandsExtensions
    {
        public static CommandsSystem UseCommands(this TOCClient client, CommandsSystemSettings? settings = null)
        {
            return new CommandsSystem(client, settings);
        }
    }
}
