using RestSharp;
using System.Text.Json;

namespace RevitDevelop.DaitoTest.Utils
{
    public class Accutils
    {
        public static async Task<TokenResponse> GetToken(string clientId, string redirect)
        {
            TokenResponse result = null;
            try
            {
                // 1) Khai báo thông tin app
                //string clientId = "<YOUR_CLIENT_ID>";
                //string redirect = "http://127.0.0.1:8123/callback";
                string scopes = "openid profile email data:read account:read viewables:read";

                // 2) Lấy token
                var oauth = new ApsPkceOAuthClient(clientId, redirect);
                result = await oauth.AcquireAccessTokenPkceAsync(scopes);

                // 3) Dùng token để gọi API (ví dụ RestSharp client khác)
                // client.AddDefaultHeader("Authorization", $"Bearer {tok.access_token}");

                // 4) Khi sắp hết hạn:
                if (result.IsExpiredSoon())
                    result = await oauth.RefreshAccessTokenAsync(result.refresh_token);
            }
            catch (Exception)
            {
            }
            return result;
        }
        public static async Task ListAccProjectsAsync(string accessToken)
        {
            // Bảo đảm TLS 1.2 nếu môi trường cũ
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            var timeout = new TimeSpan(); //modify
            var opts = new RestClientOptions("https://developer.api.autodesk.com/")
            {
                ThrowOnAnyError = false,   // ta tự kiểm tra ở GetJsonAsync
                Timeout = timeout,
            };

            var client = new RestClient(opts);
            client.AddDefaultHeader("Authorization", $"Bearer {accessToken}");

            // 1) Hubs
            using var hubsDoc = await GetJsonAsync(client, "project/v1/hubs");
            foreach (var hub in hubsDoc.RootElement.GetProperty("data").EnumerateArray())
            {
                string hubId = hub.GetProperty("id").GetString();
                string hubName = hub.GetProperty("attributes").GetProperty("name").GetString();

                // 2) Projects trong hub
                using var projsDoc = await GetJsonAsync(client, $"project/v1/hubs/{hubId}/projects");
                foreach (var p in projsDoc.RootElement.GetProperty("data").EnumerateArray())
                {
                    string projectId = p.GetProperty("id").GetString();               // ví dụ "b.******"
                    string projectName = p.GetProperty("attributes").GetProperty("name").GetString();

                    // Thử lấy projectGuid (ACC thường có trong extension)
                    string projectGuid = null;
                    if (p.TryGetProperty("attributes", out var attr)
                     && attr.TryGetProperty("extension", out var ext)
                     && ext.TryGetProperty("data", out var data)
                     && data.TryGetProperty("projectGuid", out var pg))
                    {
                        projectGuid = pg.GetString(); // GUID dùng cho Revit SaveAsCloudModel
                    }

                    Console.WriteLine($"Hub: {hubName} | Project: {projectName} | projectId: {projectId} | projectGuid: {projectGuid}");

                    // 3) Top folders (lấy folder GUID để save vào Cloud)
                    using var tops = await GetJsonAsync(client, $"project/v1/hubs/{hubId}/projects/{projectId}/topFolders");
                    foreach (var f in tops.RootElement.GetProperty("data").EnumerateArray())
                    {
                        string folderName = f.GetProperty("attributes").GetProperty("name").GetString();
                        string folderId = f.GetProperty("id").GetString(); // "urn:adsk.wipprod:fs.folder:co.{GUID}"

                        // Rút GUID từ URN:
                        string folderGuid = null;
                        int idx = folderId.LastIndexOf(":co.", StringComparison.Ordinal);
                        if (idx >= 0 && idx + 4 < folderId.Length) folderGuid = folderId.Substring(idx + 4);

                        Console.WriteLine($"    TopFolder: {folderName} | folderGuid: {folderGuid}");
                    }
                }
            }
        }
        public static async Task<JsonDocument> GetJsonAsync(RestClient client, string path)
        {
            var req = new RestRequest(path, Method.Get);
            var resp = await client.ExecuteAsync(req);
            if (!resp.IsSuccessful || resp.Content == null)
                throw new Exception($"HTTP {(int)resp.StatusCode} {resp.StatusDescription}: {resp.ErrorMessage}");

            return JsonDocument.Parse(resp.Content);
        }
    }
}
