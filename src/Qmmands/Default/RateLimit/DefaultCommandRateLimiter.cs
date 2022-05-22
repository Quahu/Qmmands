using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Qommon;

namespace Qmmands;

/// <inheritdoc/>
public partial class DefaultCommandRateLimiter : ICommandRateLimiter
{
    /// <summary>
    ///     Gets or sets the key generator of this rate-limiter.
    /// </summary>
    public Func<ICommandContext, object, object?>? BucketKeyGenerator { get; set; }

    /// <summary>
    ///     The nodes keyed by commands.
    /// </summary>
    protected readonly ConditionalWeakTable<ICommand, Node?> Nodes;

    /// <summary>
    ///     Instantiates a new <see cref="DefaultCommandRateLimiter"/>.
    /// </summary>
    public DefaultCommandRateLimiter()
    {
        Nodes = new ConditionalWeakTable<ICommand, Node?>();
    }

    /// <summary>
    ///     Creates a rate-limit bucket for the given rate-limit information.
    /// </summary>
    /// <param name="rateLimit"> The rate-limit information. </param>
    /// <returns>
    ///     A new <see cref="Bucket"/>.
    /// </returns>
    protected virtual Bucket CreateBucket(RateLimitAttribute rateLimit)
        => new(rateLimit);

    /// <summary>
    ///     Creates a rate-limit node for the given rate-limit information.
    /// </summary>
    /// <param name="rateLimits"> The rate-limit information. </param>
    /// <returns>
    ///     A new <see cref="Node"/>.
    /// </returns>
    protected virtual Node CreateNode(IEnumerable<RateLimitAttribute> rateLimits)
        => new(this, rateLimits);

    /// <inheritdoc/>
    public ValueTask<IResult> RateLimitAsync(ICommandContext context)
    {
        if (BucketKeyGenerator == null)
            return Results.Success;

        Guard.IsNotNull(context.Command);

        var command = context.Command;
        var node = Nodes.GetValue(command, command =>
        {
            var rateLimitAttributes = command.CustomAttributes.OfType<RateLimitAttribute>().ToArray();
            return rateLimitAttributes.Length != 0 ? CreateNode(rateLimitAttributes) : null;
        });

        if (node != null)
        {
            var result = node.RateLimit(context);
            return new(result);
        }

        return Results.Success;
    }
}
