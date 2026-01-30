using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace _0xOptimizer.Licensing
{
    public sealed class ApiClient
    {
        private readonly HttpClient _http;
        private readonly string _base;
        private readonly string? _apiKey;

        public ApiClient(string baseUrl, string? apiKey = null)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ArgumentException("baseUrl inválida", nameof(baseUrl));

            _base = baseUrl.Trim().TrimEnd('/');
            _apiKey = string.IsNullOrWhiteSpace(apiKey) ? null : apiKey.Trim();

            _http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(12)
            };
        }

        public async Task<ValidateResult> ValidateKeyAsync(string kid, CancellationToken ct)
        {
            kid = (kid ?? "").Trim();
            if (kid.Length < 6)
                return new ValidateResult { Valid = false, Message = "KID inválido." };

            var url = $"{_base}/api/keys/validate?kid={Uri.EscapeDataString(kid)}";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);

            using var res = await _http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, ct);
            var json = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
            {
                try
                {
                    var err = JsonSerializer.Deserialize<ApiValidateResponse>(json, JsonOpts);
                    var reason = err?.reason ?? err?.message ?? $"HTTP_{(int)res.StatusCode}";
                    return new ValidateResult { Valid = false, Message = reason };
                }
                catch
                {
                    return new ValidateResult { Valid = false, Message = $"HTTP_{(int)res.StatusCode}" };
                }
            }

            ApiValidateResponse? dto;
            try
            {
                dto = JsonSerializer.Deserialize<ApiValidateResponse>(json, JsonOpts);
            }
            catch
            {
                return new ValidateResult { Valid = false, Message = "Resposta inválida da API." };
            }

            if (dto == null)
                return new ValidateResult { Valid = false, Message = "Resposta vazia." };

            if (dto.ok != true)
                return new ValidateResult { Valid = false, Message = dto.reason ?? dto.message ?? "API erro." };

            if (dto.valid != true)
                return new ValidateResult { Valid = false, Message = dto.reason ?? "Key inválida." };

            DateTimeOffset? exp = null;
            if (!string.IsNullOrWhiteSpace(dto.expires_at) &&
                DateTimeOffset.TryParse(dto.expires_at, out var parsed))
            {
                exp = parsed.ToUniversalTime();
            }

            return new ValidateResult
            {
                Valid = true,
                Message = "OK",
                ExpiresAtUtc = exp,
                Plan = dto.plan,
                DiscordUserId = dto.discord_user_id
            };
        }

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private sealed class ApiValidateResponse
        {
            public bool? ok { get; set; }
            public bool? valid { get; set; }
            public string? reason { get; set; }
            public string? message { get; set; }
            public string? expires_at { get; set; }
            public string? plan { get; set; }
            public string? discord_user_id { get; set; }
        }
    }

    public sealed class ValidateResult
    {
        public bool Valid { get; set; }
        public string? Message { get; set; }
        public DateTimeOffset? ExpiresAtUtc { get; set; }
        public string? Plan { get; set; }
        public string? DiscordUserId { get; set; }
    }
}