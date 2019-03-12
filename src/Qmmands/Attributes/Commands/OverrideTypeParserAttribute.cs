using System;

namespace Qmmands
{
    /// <summary>
    ///     Overrides the type parser for the <see cref="Parameter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class OverrideTypeParserAttribute : Attribute
    {
        /// <summary>
        ///     Gets the <see cref="Type"/> of the custom type parser.
        /// </summary>
        public Type CustomTypeParserType { get; }

        /// <summary>
        ///     Initialises a new <see cref="OverrideTypeParserAttribute"/> with the specified custom type parser's <see cref="Type"/>.
        /// </summary>
        /// <param name="customTypeParserType"> The custom parser's <see cref="Type"/> to overwrite with. </param>
        /// <exception cref="ArgumentNullException">
        ///     Custom type parser type must not be null.
        /// </exception>
        public OverrideTypeParserAttribute(Type customTypeParserType)
            => CustomTypeParserType = customTypeParserType ?? throw new ArgumentNullException(nameof(customTypeParserType), "Custom type parser type must not be null.");
    }
}
