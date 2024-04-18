using System.Threading.Tasks;

namespace TOCSharp.Commands.Converters
{
    /// <summary>
    /// Convert nullable types
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class NullableConverter<T> : IArgumentConverter<T?> where T : struct
    {
        /// <summary>
        /// Converts the input to the desired nullable type
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Command input</param>
        /// <returns>Nullable value</returns>
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