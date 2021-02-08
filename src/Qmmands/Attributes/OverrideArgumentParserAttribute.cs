﻿using System;

namespace Qmmands
{
    /// <summary>
    ///     Overrides the argument parser for the <see cref="Module"/> or <see cref="Command"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OverrideArgumentParserAttribute : Attribute
    {
        /// <summary>
        ///     Gets the <see cref="Type"/> of the custom type parser.
        /// </summary>
        public Type Value { get; }

        /// <summary>
        ///     Initialises a new <see cref="OverrideArgumentParserAttribute"/> with the specified <see cref="Type"/> of a custom <see cref="IArgumentParser"/>.
        /// </summary>
        /// <param name="argumentParserType"> The <see cref="Type"/> to override with. </param>
        /// <exception cref="ArgumentNullException">
        ///     Custom argument parser type must not be null.
        /// </exception>
        public OverrideArgumentParserAttribute(Type argumentParserType)
        {
            if (argumentParserType == null)
                throw new ArgumentNullException(nameof(argumentParserType), "Custom type parser type must not be null.");

            Value = argumentParserType;
        }
    }
}
