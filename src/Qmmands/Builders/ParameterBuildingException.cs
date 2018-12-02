using System;

namespace Qmmands
{
    /// <summary>
    ///     Represents errors that occur during building <see cref="Parameter"/>s.
    /// </summary>
    public sealed class ParameterBuildingException : Exception
    {
        /// <summary>
        ///     Gets the <see cref="Qmmands.ParameterBuilder"/> that failed to build.
        /// </summary>
        public ParameterBuilder ParameterBuilder { get; }

        internal ParameterBuildingException(ParameterBuilder parameterBuilder, string message) : base(message)
            => ParameterBuilder = parameterBuilder;
    }
}
