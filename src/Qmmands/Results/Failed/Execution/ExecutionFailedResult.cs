using System;
using System.Reflection;

namespace Qmmands
{
    /// <summary>
    ///     Represents an execution failure.
    /// </summary>
    public sealed class ExecutionFailedResult : FailedResult
    {
        /// <summary>
        ///     Gets the reason of this failed result.
        /// </summary>s
        public override string Reason => _lazyReason.Value;
        private readonly Lazy<string> _lazyReason;

        /// <summary>
        ///     Gets the <see cref="Qmmands.Command"/> that failed to execute.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        ///     Gets the <see cref="Qmmands.CommandExecutionStep"/> at which the <see cref="Qmmands.Command"/>'s execution failed.
        /// </summary>
        public CommandExecutionStep CommandExecutionStep { get; }

        /// <summary>
        ///     Gets the <see cref="System.Exception"/> that occurred.
        /// </summary>
        public Exception Exception { get; }

        internal ExecutionFailedResult(Command command, CommandExecutionStep commandExecutionStep, Exception exception)
        {
            Command = command;
            CommandExecutionStep = commandExecutionStep;
            Exception = exception;

            while (Exception is TargetInvocationException)
                Exception = Exception.InnerException;

            _lazyReason = new Lazy<string>(() =>
            {
                switch (CommandExecutionStep)
                {
                    case CommandExecutionStep.Checks:
                        return $"An exception occurred while running checks for {Command}.";

                    case CommandExecutionStep.ArgumentParsing:
                        return $"An exception occurred while parsing raw arguments for {Command}.";

                    case CommandExecutionStep.TypeParsing:
                        return $"An exception occurred while type parsing arguments for {Command}.";

                    case CommandExecutionStep.BeforeExecuted:
                        return $"An exception occurred while calling before executed for {Command}.";

                    case CommandExecutionStep.Command:
                        return $"An exception occurred while executing {Command}.";

                    case CommandExecutionStep.CooldownBucketKeyGenerating:
                        return $"An exception occurred while generating the cooldown bucket key for {Command}.";

                    default:
                        throw new InvalidOperationException("Invalid command execution step.");
                }
            }, true);
        }
    }
}
