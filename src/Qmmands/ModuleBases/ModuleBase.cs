using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Makes the parent class a <see cref="Qmmands.Module"/> that can be added to the <see cref="CommandService"/>.
    /// </summary>
    /// <typeparam name="TContext"> The <see cref="ICommandContext"/> this <see cref="Qmmands.Module"/> will use. </typeparam>
    public abstract class ModuleBase<TContext> : IModuleBase where TContext : class, ICommandContext
    {
        /// <summary>
        ///     This <see cref="Qmmands.Module"/>.
        /// </summary>
        protected Module Module { get; private set; }

        /// <summary>
        ///     The currently executed <see cref="Qmmands.Command"/>.
        /// </summary>
        protected Command Command { get; private set; }

        /// <summary>
        ///     The context.
        /// </summary>
        protected TContext Context { get; private set; }

        /// <summary>
        ///     Fires before a <see cref="Qmmands.Command"/> in this <see cref="Qmmands.Module"/> is executed.
        /// </summary>
        /// <param name="command"> The currently executed <see cref="Qmmands.Command"/>. </param>
        protected virtual Task BeforeExecutedAsync(Command command)
            => Task.CompletedTask;

        /// <summary>
        ///     Fires after a <see cref="Qmmands.Command"/> in this <see cref="Qmmands.Module"/> is executed.
        /// </summary>
        /// <param name="command"> The currently executed <see cref="Qmmands.Command"/>. </param>
        protected virtual Task AfterExecutedAsync(Command command)
            => Task.CompletedTask;

        internal void SetContext(Module module, Command command, ICommandContext context)
        {
            Module = module;
            Command = command;
            Context = context as TContext ?? throw new InvalidOperationException($"Unable to set the context. Expected {typeof(TContext).Name}, got {context.GetType().Name}.");
        }

        Task IModuleBase.BeforeExecutedAsync(Command command)
            => BeforeExecutedAsync(command);

        Task IModuleBase.AfterExecutedAsync(Command command)
            => AfterExecutedAsync(command);

        void IModuleBase.SetContext(Module module, Command command, ICommandContext context)
            => SetContext(module, command, context);
    }
}
