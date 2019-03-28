using System;

namespace Qmmands
{
    /// <summary>
    ///     Prevents <see cref="CommandService.AddModules(System.Reflection.Assembly, Predicate{System.Reflection.TypeInfo}, Action{ModuleBuilder})"/>
    ///     from automatically adding the marked class as a <see cref="Module"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DoNotAddAttribute : Attribute
    { }
}
