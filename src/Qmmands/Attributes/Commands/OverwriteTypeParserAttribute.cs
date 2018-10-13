using System;

namespace Qmmands
{
    /// <summary>
    ///     Overwrites the type parser for the <see cref="Parameter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class OverwriteTypeParserAttribute : Attribute
    {
        /// <summary>
        ///     Gets the type parser <see cref="Type"/>.
        /// </summary>
        public Type CustomTypeParserType { get; }

        /// <summary>
        ///     Initialises a new <see cref="OverwriteTypeParserAttribute"/> with the specified custom type parser's <see cref="Type"/>.
        /// </summary>
        /// <param name="customTypeParserType"> The custom parser's <see cref="Type"/> to overwrite with. </param>
        public OverwriteTypeParserAttribute(Type customTypeParserType) 
            => CustomTypeParserType = customTypeParserType;
    }
}
