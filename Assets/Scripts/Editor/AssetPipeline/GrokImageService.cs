using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Calls the xAI (Grok) image-generation API and returns the raw PNG bytes.
    /// OpenAI-compatible images endpoint; endpoint/model overridable via SecretsConfig.
    /// </summary>
    public static class GrokImageService
    {
        public const string DefaultEndpoint = "https://api.x.ai/v1/images/generations";
        public const string DefaultModel = "grok-2-image";

        public static async Task<byte[]> GenerateAsync(string prompt, string apiKey, string model, string endpoint)
        {
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("No Grok/xAI key set.");
            string body = JsonUtility.ToJson(new Req
            {
                model = string.IsNullOrEmpty(model) ? DefaultModel : model,
                prompt = prompt,
                n = 1,
                response_format = "b64_json"
            });

            using var req = new HttpRequestMessage(HttpMethod.Post, string.IsNullOrEmpty(endpoint) ? DefaultEndpoint : endpoint);
            req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + apiKey);
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var resp = await GenerationHttp.Http.SendAsync(req);
            string json = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) throw new Exception($"Grok HTTP {(int)resp.StatusCode}: {json}");

            var parsed = JsonUtility.FromJson<Resp>(json);
            if (parsed?.data == null || parsed.data.Length == 0)
                throw new Exception("Grok returned no image. Raw: " + json);

            var d = parsed.data[0];
            if (!string.IsNullOrEmpty(d.b64_json)) return Convert.FromBase64String(d.b64_json);
            if (!string.IsNullOrEmpty(d.url)) return await GenerationHttp.Http.GetByteArrayAsync(d.url);
            throw new Exception("Grok response had neither b64_json nor url. Raw: " + json);
        }

        [Serializable] private class Req { public string model; public string prompt; public int n; public string response_format; }
        [Serializable] private class Resp { public Datum[] data; }
        [Serializable] private class Datum { public string b64_json; public string url; }
    }
}
