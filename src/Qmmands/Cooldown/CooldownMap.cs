using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Qmmands
{
    internal sealed class CooldownMap
    {
        private readonly ICooldownBucketKeyGenerator _generator;
        public ConcurrentDictionary<object, CooldownBucket> Buckets { get; }

        internal CooldownMap(ICooldownBucketKeyGenerator generator)
        {
            _generator = generator;
            Buckets = new ConcurrentDictionary<object, CooldownBucket>();
        }

        public void Update()
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var kvp in Buckets.ToImmutableArray())
            {
                if (now > kvp.Value.LastCall + kvp.Value.Cooldown.Per)
                    Buckets.TryRemove(kvp.Key, out _);
            }
        }

        public CooldownBucket GetBucket(Cooldown cooldown, ICommandContext context, IServiceProvider provider)
        {
            var key = _generator.GenerateBucketKey(cooldown.BucketType, context, provider);
            if (key is null)
                throw new InvalidOperationException("The generated cooldown key mustn't be null.");

            return Buckets.GetOrAdd(key, new CooldownBucket(cooldown));
        }
    }
}
