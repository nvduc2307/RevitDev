
using System;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RestSharp;

namespace RevitDevelop.DaitoTest.Utils
{
    public sealed class ApsPkceOAuthClient
    {
        private readonly string _clientId;
        private readonly string _redirectUri;        // ví dụ: "http://127.0.0.1:8123/callback"
        private readonly RestClient _restClient;

        public ApsPkceOAuthClient(string clientId, string redirectUri)
        {
            _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
            _redirectUri = redirectUri ?? throw new ArgumentNullException(nameof(redirectUri));
            _restClient = new RestClient("https://developer.api.autodesk.com/");
        }

        /// <summary>
        /// Lấy access_token bằng OAuth 3-legged + PKCE.
        /// </summary>
        /// <param name="scopes">Chuỗi scopes, vd: "openid profile email data:read account:read viewables:read"</param>
        /// <param name="listenPort">Mặc định 8123 (khớp redirectUri)</param>
        public async Task<TokenResponse> AcquireAccessTokenPkceAsync(string scopes, int listenPort = 8123)
        {
            // 1) Tạo PKCE verifier/challenge
            string verifier = CreateCodeVerifier();
            string challenge = CreateCodeChallenge(verifier);

            // 2) Tạo URL authorize
            string authUrl =
                "https://developer.api.autodesk.com/authentication/v2/authorize" +
                "?response_type=code" +
                $"&client_id={Uri.EscapeDataString(_clientId)}" +
                $"&redirect_uri={Uri.EscapeDataString(_redirectUri)}" +
                $"&scope={Uri.EscapeDataString(scopes)}" +
                $"&code_challenge={Uri.EscapeDataString(challenge)}" +
                "&code_challenge_method=S256";

            // 3) Mở trình duyệt để user đăng nhập
            OpenBrowser(authUrl);

            // 4) Lắng nghe redirect để lấy ?code=...
            string code = await WaitAuthCodeFromLocalCallbackAsync(listenPort).ConfigureAwait(false);
            if (string.IsNullOrEmpty(code))
                throw new InvalidOperationException("Không nhận được authorization code.");

            // 5) Đổi code → token
            var req = new RestRequest("authentication/v2/token", Method.Post);
            req.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            req.AddParameter("client_id", _clientId);
            req.AddParameter("grant_type", "authorization_code");
            req.AddParameter("code", code);
            req.AddParameter("redirect_uri", _redirectUri);
            req.AddParameter("code_verifier", verifier);

            var resp = await _restClient.ExecuteAsync(req).ConfigureAwait(false);
            EnsureSuccess(resp);

            var tok = JsonSerializer.Deserialize<TokenResponse>(resp.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (tok == null || string.IsNullOrEmpty(tok.access_token))
                throw new InvalidOperationException("Token response không hợp lệ.");

            tok.obtained_at_utc = DateTime.UtcNow;
            return tok;
        }

        /// <summary>
        /// Làm mới access_token bằng refresh_token (3-legged).
        /// </summary>
        public async Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                throw new ArgumentNullException(nameof(refreshToken));

            var req = new RestRequest("authentication/v2/token", Method.Post);
            req.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            req.AddParameter("grant_type", "refresh_token");
            req.AddParameter("client_id", _clientId);
            req.AddParameter("refresh_token", refreshToken);

            var resp = await _restClient.ExecuteAsync(req).ConfigureAwait(false);
            EnsureSuccess(resp);

            var tok = JsonSerializer.Deserialize<TokenResponse>(resp.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (tok == null || string.IsNullOrEmpty(tok.access_token))
                throw new InvalidOperationException("Refresh token response không hợp lệ.");

            tok.obtained_at_utc = DateTime.UtcNow;
            return tok;
        }

        // ================== Helpers ==================

        private static void EnsureSuccess(RestResponse resp)
        {
            if (resp.IsSuccessful && resp.Content != null) return;

            string msg = $"HTTP {(int)resp.StatusCode} {resp.StatusDescription}";
            if (!string.IsNullOrEmpty(resp.Content)) msg += $": {resp.Content}";
            if (!string.IsNullOrEmpty(resp.ErrorMessage)) msg += $"; Error: {resp.ErrorMessage}";
            throw new WebException(msg);
        }

        private static string CreateCodeVerifier()
        {
            // random 32 bytes → base64url
            var bytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Base64Url(bytes);
        }

        private static string CreateCodeChallenge(string verifier)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(verifier));
            return Base64Url(hash);
        }

        private static string Base64Url(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static void OpenBrowser(string url)
        {
            try
            {
                // .NET Framework: mở mặc định
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch
            {
                // fallback
                System.Windows.Forms.MessageBox.Show("Hãy mở URL này trong trình duyệt:\n" + url);
            }
        }

        /// <summary>
        /// Lắng nghe http://127.0.0.1:{port}/callback để lấy ?code=...
        /// </summary>
        private static async Task<string> WaitAuthCodeFromLocalCallbackAsync(int port, int timeoutMs = 120_000)
        {
            var prefix = $"http://127.0.0.1:{port}/callback/";
            using var listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();

            var ctxTask = listener.GetContextAsync();
            if (await Task.WhenAny(ctxTask, Task.Delay(timeoutMs)) != ctxTask)
            {
                listener.Stop();
                throw new TimeoutException("Hết thời gian chờ đăng nhập (no callback).");
            }

            var ctx = ctxTask.Result;
            string code = ctx.Request.QueryString["code"];

            // Trả trang báo thành công cho người dùng
            byte[] ok = Encoding.UTF8.GetBytes("<html><body>Login ok, you can close this tab.</body></html>");
            ctx.Response.ContentType = "text/html; charset=utf-8";
            ctx.Response.ContentLength64 = ok.Length;
            await ctx.Response.OutputStream.WriteAsync(ok, 0, ok.Length);
            ctx.Response.OutputStream.Close();

            listener.Stop();
            return code;
        }
    }

    /// <summary>
    /// Mô tả token trả về từ APS.
    /// </summary>
    public sealed class TokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }           // seconds (~3600)
        public string refresh_token { get; set; }     // có khi null nếu cấu hình không cấp
        public string token_type { get; set; }        // "Bearer"

        // Thời điểm lấy token (để tự tính còn hạn)
        public DateTime obtained_at_utc { get; set; }

        public bool IsExpiredSoon(int bufferSeconds = 120)
        {
            return DateTime.UtcNow >= obtained_at_utc.AddSeconds(expires_in - bufferSeconds);
        }
    }
}
