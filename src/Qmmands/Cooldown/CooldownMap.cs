using System;
using System.Collections.Concurrent;

namespace Qmmands
{
    internal sealed class CooldownMap
    {
        private readonly ConcurrentDictionary<object, CooldownBucket> _buckets;

        private readonly Command _command;

        internal CooldownMap(Command command)
        {
            _command = command;
            _buckets = new ConcurrentDictionary<object, CooldownBucket>();
        }

        public void Update()
        {
            var now = DateTimeOffset.UtcNow;
            var buckets = _buckets.ToArray();
            for (var i = 0; i < buckets.Length; i++)
            {
                var bucket = buckets[i];
                if (now > bucket.Value.LastCall + bucket.Value.Cooldown.Per)
                    _buckets.TryRemove(bucket.Key, out _);
            }
        }

        public void Clear()
            => _buckets.Clear();

        public CooldownBucket GetBucket(Cooldown cooldown, CommandContext context)
        {
            var key = _command.Service.CooldownBucketKeyGenerator(cooldown.BucketType, context);
            return key == null ? null : _buckets.GetOrAdd(key, new CooldownBucket(cooldown));
        }
    }
}
