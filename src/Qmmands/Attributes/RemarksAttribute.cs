﻿using System;

namespace Qmmands
{
    /// <summary>
    ///     Sets remarks for the <see cref="Module"/>, <see cref="Command"/>, or <see cref="Parameter"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class RemarksAttribute : Attribute
    {
        /// <summary>
        ///     Gets the remarks.
        /// </summary>
        public string Value { get; }

        /// <summary>
        ///     Initialises a new <see cref="RemarksAttribute"/> with the specified <paramref name="remarks"/>.
        /// </summary>
        /// <param name="remarks"> The value to set. </param>
        public RemarksAttribute(string remarks)
        {
            Value = remarks;
        }
    }
}
