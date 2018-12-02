using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents errors that occur during building <see cref="Module"/>s.
    /// </summary>
    public sealed class ModuleBuildingException : Exception
    {
        /// <summary>
        ///     Gets the <see cref="Qmmands.ModuleBuilder"/> that failed to build.
        /// </summary>
        public ModuleBuilder ModuleBuilder { get; }

        internal ModuleBuildingException(ModuleBuilder moduleBuilder, string message) : base(message)
            => ModuleBuilder = moduleBuilder;
    }
}
