using System;

namespace Qmmands
{
    internal sealed class CooldownBucket
    {
        public Cooldown Cooldown { get; }

        public int Remaining { get; private set; }

        public DateTimeOffset Window { get; private set; }

        public DateTimeOffset LastCall { get; private set; }

        public CooldownBucket(Cooldown cooldown)
        {
            Cooldown = cooldown;
            Remaining = Cooldown.Amount;
        }

        public bool IsRateLimited(out TimeSpan retryAfter)
        {
            var now = DateTimeOffset.UtcNow;
            LastCall = now;

            if (Remaining == Cooldown.Amount)
                Window = now;

            if (now > Window + Cooldown.Per)
            {
                Remaining = Cooldown.Amount;
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
            Remaining--;

            if (Remaining == 0)
                Window = now;
        }

        public void Reset()
        {
            Remaining = Cooldown.Amount;
            LastCall = default;
            Window = default;
        }
    }
}