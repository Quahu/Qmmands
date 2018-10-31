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
        /// <param name="command"> The <see cref="Command"/> to generate the bucket key for. </param>
        /// <param name="bucketType"> The <see langword="enum"/> bucket type. </param>
        /// <param name="context"> The <see cref="ICommandContext"/> used for execution. </param>
        /// <param name="provider"> The <see cref="IServiceProvider"/> used for execution. </param>
        /// <returns>
        ///     The <see cref="Cooldown"/> bucket key.
        /// </returns>
        object GenerateBucketKey(Command command, object bucketType, ICommandContext context, IServiceProvider provider);
    }
}
