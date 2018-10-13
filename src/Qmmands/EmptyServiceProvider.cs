using System;

namespace Qmmands
{
    internal sealed class EmptyServiceProvider : IServiceProvider
    {
        private EmptyServiceProvider()
        { }

        public object GetService(Type serviceType)
            => null;

        public static EmptyServiceProvider Instance = new EmptyServiceProvider();
    }
}
