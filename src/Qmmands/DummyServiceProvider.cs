using System;
using System.ComponentModel;

namespace Qmmands
{
    /// <summary>
    ///     Represents a dummy <see cref="IServiceProvider"/>.
    ///     Returns <see langword="null"/> for any service <see cref="Type"/> requested.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DummyServiceProvider : IServiceProvider
    {
        private DummyServiceProvider()
        { }

        /// <summary>
        ///     Returns <see langword="null"/>.
        /// </summary>
        /// <param name="serviceType"> The <see cref="Type"/> of the service to request. </param>
        /// <returns>
        ///     <see langword="null"/>
        /// </returns>
        public object GetService(Type serviceType)
            => null;

        /// <summary>
        ///     Gets the singleton instance of the <see cref="DummyServiceProvider"/>.
        /// </summary>
        public static readonly DummyServiceProvider Instance = new DummyServiceProvider();
    }
}
