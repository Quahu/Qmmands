using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Qmmands
{
    internal sealed class CooldownMap
    {
        private readonly Command _command;
        public ConcurrentDictionary<object, CooldownBucket> Buckets { get; }

        internal CooldownMap(Command command)
        {
            _command = command;
            Buckets = new ConcurrentDictionary<object, CooldownBucket>();
        }

        public CooldownBucket GetBucket(ICommandContext context, IServiceProvider provider)
        {
            var now = DateTimeOffset.UtcNow;
            foreach (var kvp in Buckets.ToList())
            {
                if (now > kvp.Value.Last + kvp.Value.Cooldown.Per)
                    Buckets.TryRemove(kvp.Key, out _);
            }

            var key = _command.Service.CooldownBucketKeyGenerator.GenerateBucketKey(_command, _command.Cooldown.BucketType, context, provider);
            return Buckets.GetOrAdd(key, new CooldownBucket(_command.Cooldown));
        }
    }
}
