using System;

namespace Qmmands
{
    /// <summary>
    ///     Sets a <see cref="Qmmands.RunMode"/> for the <see cref="Module"/> or <see cref="Command"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public sealed class RunModeAttribute : Attribute
    {
        /// <summary>
        ///     Gets the run mode.
        /// </summary>
        public RunMode RunMode { get; }

        /// <summary>
        ///     Initialises a new <see cref="RunModeAttribute"/> with the specified <see cref="Qmmands.RunMode"/>.
        /// </summary>
        /// <param name="runMode"> The <see cref="Qmmands.RunMode"/> to set. </param>
        public RunModeAttribute(RunMode runMode)
        {
            if (!Enum.IsDefined(typeof(RunMode), runMode))
                throw new ArgumentOutOfRangeException(nameof(runMode), "Invalid run mode.");

            RunMode = runMode;
        }
    }
}
