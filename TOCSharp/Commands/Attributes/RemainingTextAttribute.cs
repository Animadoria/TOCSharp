using System;

namespace TOCSharp.Commands.Attributes
{
    /// <summary>
    /// Marks that the parameter captures all the remaining text
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RemainingTextAttribute  : Attribute
    {
    }
}