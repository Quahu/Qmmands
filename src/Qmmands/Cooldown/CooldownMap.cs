using System;
using System.Collections.Concurrent;

namespace Qmmands
{
    internal sealed class CooldownMap
    {
        public ConcurrentDictionary<object, CooldownBucket> Buckets { get; }

        private readonly Command _command;

        internal CooldownMap(Command command)
        {
            _command = command;
            Buckets = new ConcurrentDictionary<object, CooldownBucket>();
        }

        public void Update()
        {
            var now = DateTimeOffset.UtcNow;
            var buckets = Buckets.ToArray();
            for (var i = 0; i < buckets.Length; i++)
            {
                var bucket = buckets[i];
                if (now > bucket.Value.LastCall + bucket.Value.Cooldown.Per)
                    Buckets.TryRemove(bucket.Key, out _);
            }
        }

        public CooldownBucket GetBucket(Cooldown cooldown, ICommandContext context, IServiceProvider provider)
        {
            var key = _command.Service.CooldownBucketKeyGenerator(_command, cooldown.BucketType, context, provider);
            return key is null ? null : Buckets.GetOrAdd(key, new CooldownBucket(cooldown));
        }
    }
}
