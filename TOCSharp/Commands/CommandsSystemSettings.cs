using System.Collections.Generic;

namespace TOCSharp.Commands
{
    public class CommandsSystemSettings
    {
        public IEnumerable<string> StringPrefixes { get; set; } = new[] { "/" };
        public char[] QuotationMarks { get; set; } = { '"' };
        public bool CaseSensitive { get; set; } = false;
    }
}