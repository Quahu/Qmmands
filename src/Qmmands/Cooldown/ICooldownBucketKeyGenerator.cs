using System;

namespace Qmmands
{
    /// <summary>
    ///     The default interface to use for generating bucket keys for <see cref="Cooldown"/>s. 
    /// </summary>
    public interface ICooldownBucketKeyGenerator
    {
        /// <summary>
        ///     Returns a <see cref="Cooldown"/> bucket key generated using the specified <paramref name="bucketType"/>.
        /// </summary>
        /// <param name="bucketType"> The bucket type. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
        /// <returns></returns>
        object GenerateBucketKey(object bucketType, ICommandContext context, IServiceProvider provider);
    }
}
