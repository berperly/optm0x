using System;

namespace _0xOptimizer.Licensing
{
    public sealed class LicenseSession
    {
        public bool IsValid { get; private set; }
        public string? Key { get; private set; }
        public DateTimeOffset? ExpiresAtUtc { get; private set; }
        public string? Plan { get; private set; }
        public string? DiscordUserId { get; private set; }

        public void SetValid(string key, ValidateResult result)
        {
            IsValid = true;
            Key = key;
            ExpiresAtUtc = result.ExpiresAtUtc;
            Plan = result.Plan;
            DiscordUserId = result.DiscordUserId;
        }

        public void Invalidate()
        {
            IsValid = false;
            Key = null;
            ExpiresAtUtc = null;
            Plan = null;
            DiscordUserId = null;
        }

        public string GetRemainingText()
        {
            if (!IsValid || ExpiresAtUtc == null) return "--";
            var remaining = ExpiresAtUtc.Value - DateTimeOffset.UtcNow;
            if (remaining.TotalSeconds <= 0) return "Expirada";
            if (remaining.TotalHours >= 1) return $"{(int)remaining.TotalHours}h {(int)remaining.Minutes}m";
            return $"{(int)remaining.TotalMinutes}m";
        }
    }
}