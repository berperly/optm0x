using _0xOptimizer.Licensing;
using System;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Threading.Timer;

namespace _0xOptimizer
{
    public sealed class LicenseService : IDisposable
    {
        private readonly ApiClient _api;
        private System.Threading.Timer? _timer;
        public string? CurrentKey { get; private set; }
        public DateTimeOffset? ExpiresAtUtc { get; private set; }

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(CurrentKey) &&
            ExpiresAtUtc != null &&
            ExpiresAtUtc > DateTimeOffset.UtcNow;

        public event Action? LicenseChanged;
        public event Action<string>? LicenseLost;

        public LicenseService(string apiBaseUrl)
        {
            _api = new ApiClient(apiBaseUrl);
        }

        public async Task<ValidateResult> LoginAsync(string key, CancellationToken ct)
        {
            var res = await _api.ValidateKeyAsync((key ?? "").Trim(), ct);
            if (res.Valid && res.ExpiresAtUtc != null)
            {
                CurrentKey = (key ?? "").Trim();
                ExpiresAtUtc = res.ExpiresAtUtc;
                LicenseChanged?.Invoke();
            }
            return res;
        }

        public void StartAutoRefresh()
        {
            StopAutoRefresh();

            _timer = new Timer(async _ =>
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(CurrentKey)) return;

                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                    var res = await _api.ValidateKeyAsync(CurrentKey!, cts.Token);

                    if (!res.Valid || res.ExpiresAtUtc == null)
                    {
                        CurrentKey = null;
                        ExpiresAtUtc = null;
                        LicenseLost?.Invoke(res.Message ?? "Licença inválida/expirada.");
                        LicenseChanged?.Invoke();
                        return;
                    }

                    ExpiresAtUtc = res.ExpiresAtUtc;
                    LicenseChanged?.Invoke();
                }
                catch { }
            }, null, TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(30));
        }

        public void StopAutoRefresh()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public string GetRemainingText()
        {
            if (ExpiresAtUtc == null) return "--";
            var remaining = ExpiresAtUtc.Value - DateTimeOffset.UtcNow;
            if (remaining.TotalSeconds <= 0) return "Expirada";
            if (remaining.TotalHours >= 1) return $"{(int)remaining.TotalHours}h {(int)remaining.Minutes}m";
            return $"{(int)remaining.TotalMinutes}m";
        }

        public void Dispose() => StopAutoRefresh();
    }
}