using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Makes the inheriting class a <see cref="Module"/> that can be added to the <see cref="CommandService"/>.
    /// </summary>
    /// <typeparam name="TContext"> The <see cref="CommandContext"/> this <see cref="Module"/> will use. </typeparam>
    public abstract class ModuleBase<TContext> : IModuleBase
        where TContext : CommandContext
    {
        /// <summary>
        ///     The execution context.
        /// </summary>
        protected TContext Context { get; private set; }

        /// <summary>
        ///     Fires before a <see cref="Command"/> in this <see cref="Module"/> is executed.
        /// </summary>
        protected virtual ValueTask BeforeExecutedAsync()
            => default;

        /// <summary>
        ///     Fires after a <see cref="Command"/> in this <see cref="Module"/> is executed.
        /// </summary>
        protected virtual ValueTask AfterExecutedAsync()
            => default;

        internal void Prepare(CommandContext context)
            => Context = context as TContext ?? throw new InvalidOperationException($"Unable to set the context. Expected {typeof(TContext)}, got {context.GetType()}.");

        ValueTask IModuleBase.BeforeExecutedAsync()
            => BeforeExecutedAsync();

        ValueTask IModuleBase.AfterExecutedAsync()
            => AfterExecutedAsync();

        void IModuleBase.Prepare(CommandContext context)
            => Prepare(context);
    }
}
