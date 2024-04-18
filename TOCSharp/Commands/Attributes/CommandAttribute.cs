using System;
using System.Collections.Generic;
using System.Linq;

namespace TOCSharp.Commands.Attributes
{
    /// <summary>
    /// Marks a method as a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        /// <summary>
        /// Names of the command
        /// </summary>
        public string[] Names { get; }

        /// <summary>
        /// Command attribute with a single name
        /// </summary>
        /// <param name="name">Name to provide</param>
        public CommandAttribute(string name)
        {
            this.Names = new[] { name };
        }

        /// <summary>
        /// Command attribute with multiple names
        /// </summary>
        /// <param name="names">Names</param>
        public CommandAttribute(IEnumerable<string> names)
        {
            this.Names = names.ToArray();
        }

        /// <summary>
        /// Command attribute with multiple names
        /// </summary>
        /// <param name="names">Names</param>
        public CommandAttribute(params string[] names)
        {
            this.Names = names;
        }
    }
}