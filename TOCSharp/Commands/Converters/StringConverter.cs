using System.Threading.Tasks;

namespace TOCSharp.Commands.Converters
{
    /// <summary>
    /// String argument converter
    /// </summary>
    public class StringConverter : IArgumentConverter<string?>
    {
        /// <summary>
        /// Convert string argument
        /// </summary>
        /// <param name="context">Command context</param>
        /// <param name="input">Input</param>
        /// <returns>Converted argument</returns>
        public Task<string?> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(input)!;
        }
    }
}