using System.Collections.Generic;

namespace TOCSharp.Commands
{
    /// <summary>
    /// Command system settings
    /// </summary>
    public class CommandsSystemSettings
    {
        /// <summary>
        /// List of prefixes
        /// </summary>
        public IEnumerable<string> StringPrefixes { get; set; } = new[] { "/" };

        /// <summary>
        /// What characters count as quotation marks to encapsulate arguments
        /// </summary>
        public char[] QuotationMarks { get; set; } = { '"' };

        /// <summary>
        /// Is case sensitive
        /// </summary>
        public bool CaseSensitive { get; set; } = false;

        /// <summary>
        /// Use default help command
        /// </summary>
        public bool UseDefaultHelpCommand { get; set; } = false;
    }
}