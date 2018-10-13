using System;

namespace Qmmands
{
    /// <summary>
    ///     Prevents <see cref="CommandService"/> from automatically injecting the property. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DontAutoInjectAttribute : Attribute
    { }
}
