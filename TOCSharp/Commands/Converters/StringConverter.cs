using System.Threading.Tasks;

namespace TOCSharp.Commands.Converters
{
    public class StringConverter : IArgumentConverter<string?>
    {
        public Task<string?> ConvertAsync(CommandContext context, string input)
        {
            return Task.FromResult(input)!;
        }
    }
}