using System;

namespace Qmmands
{
    /// <summary>
    ///     Prevents <see cref="CommandService"/> from automatically dependency injecting the marked property. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DoNotInjectAttribute : Attribute
    { }
}
