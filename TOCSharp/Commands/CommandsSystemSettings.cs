namespace TOCSharp.Commands;

public class CommandsSystemSettings
{
    public IEnumerable<string> StringPrefixes { get; init; } = new[] { "/" };
    public char[] QuotationMarks { get; init; } = ['"'];
}