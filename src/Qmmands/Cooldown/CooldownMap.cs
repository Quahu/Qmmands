﻿using System;
using System.Collections.Concurrent;
using System.Linq;

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
            foreach (var kvp in Buckets.ToList())
            {
                if (now > kvp.Value.LastCall + kvp.Value.Cooldown.Per)
                    Buckets.TryRemove(kvp.Key, out _);
            }
        }

        public CooldownBucket GetBucket(Cooldown cooldown, ICommandContext context, IServiceProvider provider)
            => Buckets.GetOrAdd(_generator.GenerateBucketKey(cooldown.BucketType, context, provider), new CooldownBucket(cooldown));
    }
}
