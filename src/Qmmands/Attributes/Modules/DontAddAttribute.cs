using System;

namespace Qmmands
{
    /// <summary>
    ///     Prevents <see cref="CommandService.AddModulesAsync(System.Reflection.Assembly)"/> from automatically adding the <see cref="Module"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DontAutoAddAttribute : Attribute
    { }
}
