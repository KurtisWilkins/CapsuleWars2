using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Calls the Anthropic Messages API to write an item description. Endpoint/model are
    /// overridable via SecretsConfig (see GenerationServices) in case the defaults drift.
    /// </summary>
    public static class AnthropicDescriptionService
    {
        public const string DefaultEndpoint = "https://api.anthropic.com/v1/messages";
        public const string DefaultModel = "claude-sonnet-4-6";
        private const string AnthropicVersion = "2023-06-01";

        public static async Task<string> GenerateAsync(string prompt, string apiKey, string model, string endpoint)
        {
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("No Anthropic key set.");
            string body = JsonUtility.ToJson(new Req
            {
                model = string.IsNullOrEmpty(model) ? DefaultModel : model,
                max_tokens = 600,
                messages = new[] { new Msg { role = "user", content = prompt } }
            });

            using var req = new HttpRequestMessage(HttpMethod.Post, string.IsNullOrEmpty(endpoint) ? DefaultEndpoint : endpoint);
            req.Headers.TryAddWithoutValidation("x-api-key", apiKey);
            req.Headers.TryAddWithoutValidation("anthropic-version", AnthropicVersion);
            req.Content = new StringContent(body, Encoding.UTF8, "application/json");

            var resp = await GenerationHttp.Http.SendAsync(req);
            string json = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) throw new Exception($"Anthropic HTTP {(int)resp.StatusCode}: {json}");

            var parsed = JsonUtility.FromJson<Resp>(json);
            if (parsed?.content == null || parsed.content.Length == 0)
                throw new Exception("Anthropic returned no content. Raw: " + json);
            return parsed.content[0].text;
        }

        [Serializable] private class Req { public string model; public int max_tokens; public Msg[] messages; }
        [Serializable] private class Msg { public string role; public string content; }
        [Serializable] private class Resp { public Content[] content; }
        [Serializable] private class Content { public string type; public string text; }
    }
}
