using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TOCSharp.Commands.Attributes;

namespace TOCSharp.Commands
{
    public class DefaultHelpCommand : ICommandModule
    {
        [Command("help")]
        public async Task Help(CommandContext ctx)
        {
            IEnumerable<CommandInfo> commands = ctx.CommandsSystem.Commands.Values.Distinct();
            string message = "Here are the command available on this bot:\n";
            foreach (CommandInfo command in commands)
            {
                if (command.Attribute.Names.Length == 0)
                {
                    continue;
                }

                string aliases = "";
                if (command.Attribute.Names.Length > 1)
                {
                    aliases = $" (aliases: {string.Join(", ", command.Attribute.Names.Skip(1))})";
                }

                message += $"{command.Attribute.Names[0]}{aliases}; ";
            }

            await ctx.ReplyAsync(message.Trim().Trim(';'));
        }
    }
}