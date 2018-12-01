using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Makes the inheriting class a <see cref="Module"/> that can be added to the <see cref="CommandService"/>.
    /// </summary>
    /// <typeparam name="TContext"> The <see cref="ICommandContext"/> this <see cref="Module"/> will use. </typeparam>
    public abstract class ModuleBase<TContext> : IModuleBase where TContext : class, ICommandContext
    {
        /// <summary>
        ///     The command context.
        /// </summary>
        protected TContext Context { get; private set; }

        /// <summary>
        ///     Fires before a <see cref="Command"/> in this <see cref="Module"/> is executed.
        /// </summary>
        /// <param name="command"> The about to be executed <see cref="Command"/>. </param>
        protected virtual Task BeforeExecutedAsync(Command command)
            => Task.CompletedTask;

        /// <summary>
        ///     Fires after a <see cref="Command"/> in this <see cref="Module"/> is executed.
        /// </summary>
        /// <param name="command"> The executed <see cref="Command"/>. </param>
        protected virtual Task AfterExecutedAsync(Command command)
            => Task.CompletedTask;

        internal void Prepare(ICommandContext context)
            => Context = context as TContext ?? throw new InvalidOperationException($"Unable to set the context. Expected {typeof(TContext)}, got {context.GetType()}.");

        Task IModuleBase.BeforeExecutedAsync(Command command)
            => BeforeExecutedAsync(command);

        Task IModuleBase.AfterExecutedAsync(Command command)
            => AfterExecutedAsync(command);

        void IModuleBase.Prepare(ICommandContext context)
            => Prepare(context);
    }
}
