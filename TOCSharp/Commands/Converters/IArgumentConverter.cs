using System.Threading.Tasks;

namespace TOCSharp.Commands.Converters
{
    /// <summary>
    /// Argument converter interface
    /// </summary>
    public interface IArgumentConverter
    {
    }

    /// <summary>
    /// Generic argument converter interface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IArgumentConverter<T> : IArgumentConverter
    {
        /// <summary>
        /// Converts the input to the desired type
        /// </summary>
        /// <param name="context">Context of the command</param>
        /// <param name="input">Input</param>
        /// <returns>Task of returned generic type</returns>
        public Task<T> ConvertAsync(CommandContext context, string input);
    }
}