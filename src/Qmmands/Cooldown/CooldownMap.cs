using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Qmmands
{
    internal sealed class CooldownMap
    {
        public ConcurrentDictionary<object, CooldownBucket> Buckets { get; }

        private readonly Command _command;
        private readonly ICooldownBucketKeyGenerator _generator;

        internal CooldownMap(Command command, ICooldownBucketKeyGenerator generator)
        {
            _command = command;
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
            var key = _generator.GenerateBucketKey(_command, cooldown.BucketType, context, provider);
            return key is null ? null : Buckets.GetOrAdd(key, new CooldownBucket(cooldown));
        }
    }
}
