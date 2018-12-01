using System;

namespace Qmmands
{
    /// <summary>
    ///     Overwrites the type parser for the <see cref="Parameter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class OverrideTypeParserAttribute : Attribute
    {
        /// <summary>
        ///     Gets the type parser <see cref="Type"/>.
        /// </summary>
        public Type CustomTypeParserType { get; }

        /// <summary>
        ///     Initialises a new <see cref="OverrideTypeParserAttribute"/> with the specified custom type parser's <see cref="Type"/>.
        /// </summary>
        /// <param name="customTypeParserType"> The custom parser's <see cref="Type"/> to overwrite with. </param>
        /// <exception cref="ArgumentNullException">
        ///     Custom type parser type mustn't be null.
        /// </exception>
        public OverrideTypeParserAttribute(Type customTypeParserType)
            => CustomTypeParserType = customTypeParserType ?? throw new ArgumentNullException(nameof(customTypeParserType), "Custom type parser type mustn't be null.");
    }
}
