using System;

namespace Qmmands
{
    /// <summary>
    ///     Marks the <see cref="Parameter"/> as a remainder parameter.
    /// </summary>
    /// <remarks>
    ///     Remainder parameters are the last parameters of <see cref="Command"/>s.
    ///     Using the <see cref="DefaultArgumentParser"/> remainder parameters can consist
    ///     of multiple words without the need of using quotation marks."/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RemainderAttribute : Attribute
    { }
}
