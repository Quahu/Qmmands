using System;
using System.Reflection;

namespace Qmmands
{
    /// <summary>
    ///     Represents an execution failure.
    /// </summary>
    public sealed class ExecutionFailedResult : FailedResult
    {
        /// <inheritdoc />
        public override string Reason { get; }

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

            switch (CommandExecutionStep)
            {
                case CommandExecutionStep.Checks:
                    Reason = $"An exception occurred while running checks for {Command}.";
                    break;

                case CommandExecutionStep.ArgumentParsing:
                    Reason = $"An exception occurred while parsing raw arguments for {Command}.";
                    break;

                case CommandExecutionStep.TypeParsing:
                    Reason = $"An exception occurred while type parsing arguments for {Command}.";
                    break;

                case CommandExecutionStep.BeforeExecuted:
                    Reason = $"An exception occurred while calling before executed for {Command}.";
                    break;

                case CommandExecutionStep.Command:
                    Reason = $"An exception occurred while executing {Command}.";
                    break;
            }
        }
    }
}
