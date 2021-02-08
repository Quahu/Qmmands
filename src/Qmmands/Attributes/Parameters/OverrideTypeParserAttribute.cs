using System;

namespace Qmmands
{
    /// <summary>
    ///     Overrides the type parser for the <see cref="Parameter"/>.
    /// </summary>
    /// <remarks>
    ///    The type parser must still be added to the <see cref="CommandService"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class OverrideTypeParserAttribute : Attribute
    {
        /// <summary>
        ///     Gets the <see cref="Type"/> of the custom type parser.
        /// </summary>
        public Type Value { get; }

        /// <summary>
        ///     Initialises a new <see cref="OverrideTypeParserAttribute"/> with the specified <see cref="Type"/> of a custom <see cref="TypeParser{T}"/>.
        /// </summary>
        /// <param name="typeParserType"> The <see cref="Type"/> to override with. </param>
        /// <exception cref="ArgumentNullException">
        ///     Custom type parser type must not be null.
        /// </exception>
        public OverrideTypeParserAttribute(Type typeParserType)
        {
            if (typeParserType == null)
                throw new ArgumentNullException(nameof(typeParserType), "Custom type parser type must not be null.");

            Value = typeParserType;
        }
    }
}
