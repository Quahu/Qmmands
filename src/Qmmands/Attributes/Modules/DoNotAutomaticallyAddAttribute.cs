using System;

namespace Qmmands
{
    /// <summary>
    ///     Prevents <see cref="CommandService.AddModules(System.Reflection.Assembly)"/> from automatically adding the marked class as a <see cref="Module"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotAutomaticallyAddAttribute : Attribute
    { }
}
