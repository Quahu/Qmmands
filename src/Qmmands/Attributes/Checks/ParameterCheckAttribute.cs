using System;
using System.Threading.Tasks;

namespace Qmmands
{
    /// <summary>
    ///     Represents a <see cref="Qmmands.Parameter"/> check that has to succeed before the <see cref="Command"/> can be executed.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public abstract class ParameterCheckAttribute : Attribute
    {
        /// <summary>
        ///     Gets the <see cref="Qmmands.Parameter"/> this <see cref="ParameterCheckAttribute"/> is for.
        /// </summary>
        public Parameter Parameter { get; internal set; }


        /// <inheritdoc cref="CheckAttribute.Group"/>
        public object Group { get; set; }
        
        /// <summary>
        ///     Gets or sets whether this check should apply to arrays as a whole or its individual elements.
        ///     This only applies to parameters with <see cref="Qmmands.Parameter.IsMultiple"/> set to <see langword="true"/>.
        /// </summary>
        public bool ChecksArrayElements { get; set; }

        /// <summary>
        ///     Determines which types are supported by this <see cref="ParameterCheckAttribute"/>.
        /// </summary>
        public virtual bool CheckType(Type type)
            => true;

        /// <summary>
        ///     Determines whether the <paramref name="argument"/> is valid for the <see cref="Qmmands.Parameter"/> in given circumstances.
        /// </summary>
        /// <param name="argument"> The value given to this <see cref="Qmmands.Parameter"/>. </param>
        /// <param name="context"> The <see cref="CommandContext"/> used during execution. </param>
        /// <returns>
        ///     A <see cref="CheckResult"/> which determines whether this <see cref="ParameterCheckAttribute"/> succeeded or not.
        /// </returns>
        public abstract ValueTask<CheckResult> CheckAsync(object argument, CommandContext context);

        /// <summary>
        ///     Returns a successful <see cref="CheckResult"/>.
        /// </summary>
        /// <returns> A successful <see cref="CheckResult"/>. </returns>
        protected static CheckResult Success()
            => CheckResult.Successful;

        /// <summary>
        ///     Returns a failed <see cref="CheckResult"/> with the specified reason.
        /// </summary>
        /// <returns> A failed <see cref="CheckResult"/>. </returns>
        protected static CheckResult Failure(string reason)
            => CheckResult.Failed(reason);
    }
}
