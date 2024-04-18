using System;
using System.Threading.Tasks;

namespace TOCSharp
{
    /// <summary>
    /// Async Event Handler with generic parameters
    /// </summary>
    /// <typeparam name="T">Generic param</typeparam>
    public delegate Task AsyncEventHandler<in T>(object sender, T args);

    /// <summary>
    /// Async Event Handler
    /// </summary>
    public delegate Task AsyncEventHandler(object sender, EventArgs args);
}