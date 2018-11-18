using System;

namespace Qmmands
{
    /// <summary>
    ///     Sets the <see cref="Parameter"/> as the remainder.
    /// </summary>
    /// <remarks>
    ///     Remainder parameters are the last parameters of a <see cref="Command"/>.
    ///     Using the <see cref="DefaultArgumentParser"/> remainder parameters can consist
    ///     of multiple words without the need of using quotation marks."/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class RemainderAttribute : Attribute
    { }
}
