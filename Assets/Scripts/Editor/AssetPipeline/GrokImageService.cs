using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Calls the xAI (Grok) image API and returns the raw PNG bytes. Two paths:
    /// <see cref="GenerateAsync"/> = text→image (POST /v1/images/generations);
    /// <see cref="EditAsync"/> = reference image + prompt (POST /v1/images/edits, opt-in/experimental).
    /// Endpoints/model overridable via SecretsConfig. xAI has no seed param (June 2026), so
    /// consistency relies on the prompt + fixed aspect_ratio/resolution.
    /// </summary>
    public static class GrokImageService
    {
        public const string DefaultEndpoint = "https://api.x.ai/v1/images/generations";
        public const string DefaultEditEndpoint = "https://api.x.ai/v1/images/edits";
        public const string DefaultModel = "grok-imagine-image-quality";

        public static async Task<byte[]> GenerateAsync(
            string prompt, string apiKey, string model, string endpoint, string aspectRatio, string resolution)
        {
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("No Grok/xAI key set.");
            string body = JsonUtility.ToJson(new GenReq
            {
                model = Model(model),
                prompt = prompt,
                n = 1,
                response_format = "b64_json",
                aspect_ratio = string.IsNullOrEmpty(aspectRatio) ? "1:1" : aspectRatio,
                resolution = string.IsNullOrEmpty(resolution) ? "1k" : resolution
            });
            return await PostForImage(string.IsNullOrEmpty(endpoint) ? DefaultEndpoint : endpoint, apiKey, body);
        }

        /// <summary>
        /// Reference-image path (experimental). xAI edits MODIFY the source image, so this is a
        /// best-effort style anchor — the exact `image` field shape may need adjusting per xAI docs.
        /// </summary>
        public static async Task<byte[]> EditAsync(
            string prompt, byte[] referencePng, string apiKey, string model, string endpoint, string aspectRatio, string resolution)
        {
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("No Grok/xAI key set.");
            if (referencePng == null || referencePng.Length == 0) throw new Exception("No reference image bytes.");
            // xAI /v1/images/edits wants `image` as an OBJECT {url, type:"image_url"} — a base64 data URI is
            // accepted in `url`. The old code sent `image` as a bare string → HTTP 422 "image: invalid type:
            // string". (docs.x.ai → model-capabilities/images/editing)
            string body = JsonUtility.ToJson(new EditReq
            {
                model = Model(model),
                prompt = prompt,
                image = new ImageRef { url = "data:image/png;base64," + Convert.ToBase64String(referencePng), type = "image_url" },
                response_format = "b64_json"
            });
            return await PostForImage(string.IsNullOrEmpty(endpoint) ? DefaultEditEndpoint : endpoint, apiKey, body);
        }

        private static string Model(string model) => string.IsNullOrEmpty(model) ? DefaultModel : model;

        private static async Task<byte[]> PostForImage(string url, string apiKey, string body)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
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

        [Serializable] private class GenReq { public string model; public string prompt; public int n; public string response_format; public string aspect_ratio; public string resolution; }
        [Serializable] private class EditReq { public string model; public string prompt; public ImageRef image; public string response_format; }
        [Serializable] private class ImageRef { public string url; public string type; }
        [Serializable] private class Resp { public Datum[] data; }
        [Serializable] private class Datum { public string b64_json; public string url; }
    }
}
