using System;
using System.Threading;

namespace Qmmands
{
    internal sealed class CooldownBucket
    {
        public Cooldown Cooldown { get; }

        private int _remaining;

        public int Remaining
            => Volatile.Read(ref _remaining);

        public DateTimeOffset Window { get; private set; }

        public DateTimeOffset LastCall { get; private set; }

        public CooldownBucket(Cooldown cooldown)
        {
            Cooldown = cooldown;
            _remaining = Cooldown.Amount;
        }

        public bool IsRateLimited(out TimeSpan retryAfter)
        {
            var now = DateTimeOffset.UtcNow;
            LastCall = now;

            if (Remaining == Cooldown.Amount)
                Window = now;

            if (now > Window + Cooldown.Per)
            {
                _remaining = Cooldown.Amount;
                Window = now;
            }

            if (Remaining == 0)
            {
                retryAfter = Cooldown.Per - (now - Window);
                return true;
            }

            retryAfter = default;
            return false;
        }

        public void Decrement()
        {
            var now = DateTimeOffset.UtcNow;
            Interlocked.Decrement(ref _remaining);

            if (Remaining == 0)
                Window = now;
        }

        public void Reset()
        {
            _remaining = Cooldown.Amount;
            LastCall = default;
            Window = default;
        }
    }
}