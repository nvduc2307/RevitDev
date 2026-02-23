using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDevelop.DaitoTest.Utils
{
    public static class acUtils
    {
        // Tạo PKCE
        public static string Verifier()
        {
            var b = new byte[32];
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            rng.GetBytes(b);
            return Convert.ToBase64String(b).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
        public static string Challenge(string v)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(v));
            return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        public static async Task<string> WaitCodeAsync(int port = 8123)
        {
            using var l = new System.Net.HttpListener();
            l.Prefixes.Add($"http://127.0.0.1:{port}/callback/");
            l.Start();
            var ctx = await l.GetContextAsync();
            string code = ctx.Request.QueryString["code"];
            byte[] ok = System.Text.Encoding.UTF8.GetBytes("Login ok. You can close this tab.");
            ctx.Response.OutputStream.Write(ok, 0, ok.Length);
            ctx.Response.OutputStream.Close();
            l.Stop();
            return code;
        }

        public static async Task<(string accessToken, string refreshToken)> SignInPkceAsync(
            string clientId, string redirectUri,
            string scopes = "openid profile email data:read account:read viewables:read")
        {
            var verifier = Verifier();
            var challenge = Challenge(verifier);

            var authUrl =
              "https://developer.api.autodesk.com/authentication/v2/authorize" +
              $"?response_type=code&client_id={Uri.EscapeDataString(clientId)}" +
              $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
              $"&scope={Uri.EscapeDataString(scopes)}" +
              $"&code_challenge={challenge}&code_challenge_method=S256";

            // mở trình duyệt hệ thống
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = authUrl,
                UseShellExecute = true
            });

            // chờ code
            int port = new Uri(redirectUri).Port;
            string code = await WaitCodeAsync(port);

            // đổi code -> token
            var client = new RestSharp.RestClient("https://developer.api.autodesk.com/");
            var req = new RestSharp.RestRequest("authentication/v2/token", Method.Post)
                .AddHeader("Content-Type", "application/x-www-form-urlencoded")
                .AddParameter("grant_type", "authorization_code")
                .AddParameter("client_id", clientId)
                .AddParameter("code", code)
                .AddParameter("redirect_uri", redirectUri)
                .AddParameter("code_verifier", verifier);

            var resp = await client.ExecuteAsync(req);
            if (!resp.IsSuccessful) throw new Exception(resp.Content);

            using var doc = System.Text.Json.JsonDocument.Parse(resp.Content);
            string access = doc.RootElement.GetProperty("access_token").GetString();
            string refresh = doc.RootElement.TryGetProperty("refresh_token", out var r) ? r.GetString() : null;
            return (access, refresh);
        }

        // Refresh token:
        public static async Task<string> RefreshAsync(string clientId, string refreshToken)
        {
            var client = new RestSharp.RestClient("https://developer.api.autodesk.com/");
            var req = new RestSharp.RestRequest("authentication/v2/token", Method.Post)
                .AddHeader("Content-Type", "application/x-www-form-urlencoded")
                .AddParameter("grant_type", "refresh_token")
                .AddParameter("client_id", clientId)
                .AddParameter("refresh_token", refreshToken);

            var resp = await client.ExecuteAsync(req);
            if (!resp.IsSuccessful) throw new Exception(resp.Content);

            using var doc = System.Text.Json.JsonDocument.Parse(resp.Content);
            return doc.RootElement.GetProperty("access_token").GetString();
        }
    }
}
