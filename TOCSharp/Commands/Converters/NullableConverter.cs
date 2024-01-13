using System.Threading.Tasks;

namespace TOCSharp.Commands.Converters
{
    public class NullableConverter<T> : IArgumentConverter<T?> where T : struct
    {
        public async Task<T?> ConvertAsync(CommandContext context, string input)
    {
        if (input == "null")
        {
            return null;
        }

        if (context.CommandsSystem.ArgumentConverters.TryGetValue(typeof(T), out IArgumentConverter? cvt))
        {
            IArgumentConverter<T> cvx = (IArgumentConverter<T>)cvt;
            return await cvx.ConvertAsync(context, input);
        }

        return default;
    }
    }
}