using System;
using System.Collections.Generic;
using System.Linq;

namespace TOCSharp.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string[] Names { get; }

        public CommandAttribute(string name)
        {
            this.Names = new[] { name };
        }

        public CommandAttribute(IEnumerable<string> names)
    {
        this.Names = names.ToArray();
    }

        public CommandAttribute(params string[] names)
    {
        this.Names = names;
    }
    }
}