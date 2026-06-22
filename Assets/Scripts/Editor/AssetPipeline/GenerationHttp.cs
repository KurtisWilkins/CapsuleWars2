using System;
using System.Collections.Generic;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace CapsuleWars.Editor.AssetPipeline
{
    /// <summary>
    /// Shared <see cref="HttpClient"/> for the generation services + a pump that runs
    /// queued actions on Unity's main thread. Network work happens on the thread pool
    /// (async/await); anything touching the AssetDatabase or ScriptableObjects must be
    /// marshaled back via <see cref="OnMainThread"/>. Editor-only.
    /// </summary>
    public static class GenerationHttp
    {
        private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromMinutes(10) };
        private static readonly Queue<Action> Pending = new Queue<Action>();

        public static HttpClient Http => Client;

        [InitializeOnLoadMethod]
        private static void Init()
        {
            // Registered once on the main thread; drains the queue every editor tick.
            EditorApplication.update -= Pump;
            EditorApplication.update += Pump;
        }

        /// <summary>Queue an action to run on the main thread (safe to call from any thread).</summary>
        public static void OnMainThread(Action action)
        {
            if (action == null) return;
            lock (Pending) Pending.Enqueue(action);
        }

        private static void Pump()
        {
            while (true)
            {
                Action action;
                lock (Pending)
                {
                    if (Pending.Count == 0) return;
                    action = Pending.Dequeue();
                }
                try { action(); }
                catch (Exception e) { Debug.LogException(e); }
            }
        }

        /// <summary>Unwrap an AggregateException from a faulted Task to its real cause.</summary>
        public static Exception Unwrap(Exception e)
        {
            if (e is AggregateException ae) return ae.Flatten().InnerException ?? ae;
            return e;
        }
    }
}
