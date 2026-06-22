using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Calls Meshy's image-to-3D API: create a task from the chosen image, poll until it
    /// succeeds, then download the model. Returns the model bytes + file extension.
    /// Endpoint/model overridable via SecretsConfig. Async + long-running (minutes).
    /// </summary>
    public static class MeshyModelService
    {
        public const string DefaultCreateEndpoint = "https://api.meshy.ai/openapi/v1/image-to-3d";
        public const string DefaultAiModel = "meshy-5";

        private const int MaxPolls = 240;     // ~20 min at 5s
        private const int PollDelayMs = 5000;

        public static async Task<ModelResult> GenerateAsync(
            byte[] imagePng, string apiKey, string aiModel, string endpoint, Action<string> progress)
        {
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("No Meshy key set.");
            string createUrl = string.IsNullOrEmpty(endpoint) ? DefaultCreateEndpoint : endpoint;
            string dataUri = "data:image/png;base64," + Convert.ToBase64String(imagePng);

            string createBody = JsonUtility.ToJson(new CreateReq
            {
                image_url = dataUri,
                ai_model = string.IsNullOrEmpty(aiModel) ? DefaultAiModel : aiModel,
                should_texture = true,
                enable_pbr = false
            });

            // 1. Create the task.
            string taskId;
            using (var req = new HttpRequestMessage(HttpMethod.Post, createUrl))
            {
                req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + apiKey);
                req.Content = new StringContent(createBody, Encoding.UTF8, "application/json");
                var resp = await GenerationHttp.Http.SendAsync(req);
                string json = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode) throw new Exception($"Meshy create HTTP {(int)resp.StatusCode}: {json}");
                var created = JsonUtility.FromJson<CreateResp>(json);
                taskId = created != null && !string.IsNullOrEmpty(created.result) ? created.result : null;
                if (string.IsNullOrEmpty(taskId)) throw new Exception("Meshy create returned no task id. Raw: " + json);
            }

            // 2. Poll.
            string getUrl = createUrl + "/" + taskId;
            for (int i = 0; i < MaxPolls; i++)
            {
                await Task.Delay(PollDelayMs);
                using var req = new HttpRequestMessage(HttpMethod.Get, getUrl);
                req.Headers.TryAddWithoutValidation("Authorization", "Bearer " + apiKey);
                var resp = await GenerationHttp.Http.SendAsync(req);
                string json = await resp.Content.ReadAsStringAsync();
                if (!resp.IsSuccessStatusCode) throw new Exception($"Meshy poll HTTP {(int)resp.StatusCode}: {json}");

                var st = JsonUtility.FromJson<TaskResp>(json);
                progress?.Invoke($"Meshy: {st.status} {st.progress}%");

                if (st.status == "SUCCEEDED")
                {
                    string url = st.model_urls != null ? st.model_urls.fbx : null;
                    string ext = "fbx";
                    if (string.IsNullOrEmpty(url) && st.model_urls != null) { url = st.model_urls.glb; ext = "glb"; }
                    if (string.IsNullOrEmpty(url)) throw new Exception("Meshy succeeded but returned no fbx/glb url. Raw: " + json);
                    byte[] data = await GenerationHttp.Http.GetByteArrayAsync(url);
                    return new ModelResult { data = data, ext = ext };
                }
                if (st.status == "FAILED" || st.status == "EXPIRED" || st.status == "CANCELED")
                    throw new Exception($"Meshy task {st.status}. Raw: {json}");
            }
            throw new Exception("Meshy timed out waiting for the model.");
        }

        public struct ModelResult { public byte[] data; public string ext; }

        [Serializable] private class CreateReq { public string image_url; public string ai_model; public bool should_texture; public bool enable_pbr; }
        [Serializable] private class CreateResp { public string result; }
        [Serializable] private class TaskResp { public string status; public int progress; public ModelUrls model_urls; }
        [Serializable] private class ModelUrls { public string fbx; public string glb; public string obj; public string usdz; }
    }
}
