using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _0xOptimizer.Licensing
{
    public sealed class LicenseGate : IDisposable
    {
        private readonly ApiClient _api;
        private readonly LicenseSession _session;

        private System.Windows.Forms.Timer? _refreshTimer;

        public event Action<string>? StatusChanged; // status pro UI (opcional)
        public event Action? SessionInvalidated;    // quando expirar/invalidate

        public LicenseGate(ApiClient api, LicenseSession session)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public async Task<ValidateResult> LoginAsync(string kid, CancellationToken ct)
        {
            var res = await _api.ValidateKeyAsync(kid, ct);

            if (!res.Valid || res.ExpiresAtUtc == null)
            {
                _session.Invalidate();
                StopRefresh();
                return res;
            }

            _session.SetValid(kid, res);
            StartRefresh();
            StatusChanged?.Invoke("Licença validada.");
            return res;
        }

        public void Logout(string? reason = null)
        {
            _session.Invalidate();
            StopRefresh();
            if (!string.IsNullOrWhiteSpace(reason))
                StatusChanged?.Invoke(reason);
            SessionInvalidated?.Invoke();
        }

        private void StartRefresh()
        {
            StopRefresh();

            if (!_session.IsValid || string.IsNullOrWhiteSpace(_session.Key))
                return;

            _refreshTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
            _refreshTimer.Tick += async (_, __) =>
            {
                try
                {
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                    var res = await _api.ValidateKeyAsync(_session.Key!, cts.Token);

                    if (!res.Valid || res.ExpiresAtUtc == null)
                    {
                        Logout("Licença inválida/expirada.");
                        return;
                    }

                    _session.SetValid(_session.Key!, res);
                }
                catch
                {
                    // silencioso
                }
            };
            _refreshTimer.Start();
        }

        private void StopRefresh()
        {
            if (_refreshTimer == null) return;
            try { _refreshTimer.Stop(); } catch { }
            try { _refreshTimer.Dispose(); } catch { }
            _refreshTimer = null;
        }

        public void Dispose() => StopRefresh();
    }
}