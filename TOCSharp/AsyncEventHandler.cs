using System;
using System.Threading.Tasks;

namespace TOCSharp
{
    public delegate Task AsyncEventHandler<in T>(object sender, T args);

    public delegate Task AsyncEventHandler(object sender, EventArgs args);
}