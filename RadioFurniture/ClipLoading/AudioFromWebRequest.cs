using RadioFurniture.ClipLoading;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using WebRequestAudio.ClipLoading;

namespace RadioFurniture.ClipLoading
{
    /// <summary>
    /// Load Audio from Url
    /// </summary>
    public static class AudioFromWebRequest
    {
        /// <summary>
        /// Async operation to load audio clip. For streaming - see overload.
        /// </summary>
        /// <param name="url">can be local Uri or remote URI</param>
        /// <returns>TaskCompletionSource with Task to await for</returns>
        public static TaskCompletionSource<DisposableAudioWebRequest> LoadAudioFrom(
            AudioSource audioSource,
            string url,
            Dictionary<string, string> headers,
            AudioType typeOfAudio)
        {
            var taskSource = new TaskCompletionSource<DisposableAudioWebRequest>();
            LoadStart(audioSource, url, headers, typeOfAudio, false, 1024, CancellationToken.None,
                taskSource);
            return taskSource;
        }

        /// <summary>
        /// Async operation to load audio clip
        /// </summary>
        /// <param name="url">can be local Uri or remote URI</param>
        /// <param name="enableStreaming">to use streaming audio</param>
        /// <param name="minimumKbForStreaming">required if streaming, 1024Kb is recommended</param>
        /// <returns>TaskCompletionSource with Task to await for</returns>
        public static TaskCompletionSource<DisposableAudioWebRequest> LoadAudioFrom(
            AudioSource audioSource,
            string url,
            Dictionary<string, string> headers,
            AudioType typeOfAudio,
            bool enableStreaming,
            int minimumKbForStreaming,
            CancellationToken token)
        {
            var taskSource = new TaskCompletionSource<DisposableAudioWebRequest>();
            LoadStart(audioSource, url, headers, typeOfAudio, enableStreaming, minimumKbForStreaming, token,
                taskSource);
            return taskSource;
        }

        private static async void LoadStart(
            AudioSource audioSource,
            string url,
            Dictionary<string, string> headers,
            AudioType typeOfAudio,
            bool enableStreaming,
            int minimumKbForStreaming,
            CancellationToken token,
            TaskCompletionSource<DisposableAudioWebRequest> taskSource)
        {
            try
            {
                var cancellationTokenSource = new CancellationTokenSource();
                System.Action actionOnCancel = () => { cancellationTokenSource.Cancel(); };
                token.Register(() => { actionOnCancel?.Invoke(); });
                var webRequest = await LoadAudioFileAsync(
                    url,
                    headers,
                    typeOfAudio,
                    enableStreaming,
                    minimumKbForStreaming,
                    cancellationTokenSource.Token);

                using var disposableWebRequest =
                    TryGetRequestWithClip(webRequest, audioSource, cancellationTokenSource.Token);

                if (token.IsCancellationRequested || !string.IsNullOrEmpty(webRequest.error))
                {
                    disposableWebRequest.Dispose();
                    taskSource.TrySetResult(disposableWebRequest);
                    return;
                }

                if (enableStreaming)
                {
                    taskSource.TrySetResult(disposableWebRequest);
                }

                if (webRequest.isDone)
                {
                    taskSource.TrySetResult(disposableWebRequest);
                }

                await WaitForDispose(webRequest, cancellationTokenSource.Token);
                actionOnCancel = null;
                disposableWebRequest.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static DisposableAudioWebRequest TryGetRequestWithClip(
            UnityWebRequest webRequest,
            AudioSource audioSource,
            CancellationToken token)
        {
            var disposableWebRequest = new DisposableAudioWebRequest(webRequest);
            if (!token.IsCancellationRequested)
            {
                disposableWebRequest.SetStatus();
            }

            if (token.IsCancellationRequested || !string.IsNullOrEmpty(webRequest.error))
            {
                return disposableWebRequest;
            }

            var clip = ((DownloadHandlerAudioClip)webRequest.downloadHandler).audioClip;
            var disposableAudioClip = new DisposableAudioClip(audioSource, clip);
            disposableAudioClip.SetToken(token);
            disposableWebRequest.SetDisposableClip(disposableAudioClip);
            return disposableWebRequest;
        }

        private static async Task WaitForDispose(UnityWebRequest request, CancellationToken token)
        {
            while (!token.IsCancellationRequested && !request.isDone && string.IsNullOrEmpty(request.error))
            {
                await Task.Delay(500);
            }
        }

        private static async Task<UnityWebRequest> LoadAudioFileAsync(
            string url,
            Dictionary<string, string> headers,
            AudioType typeOfAudio,
            bool enableStreaming,
            int minimumKbForStreaming,
            CancellationToken token)
        {
            var request = UnityWebRequestMultimedia.GetAudioClip(url, typeOfAudio);
            var taskSource = new TaskCompletionSource<bool>();
            token.Register(() =>
            {
                if (request != null)
                {
                    request.Abort();
                }

                taskSource.TrySetResult(false);
            });
            if (headers != null)
            {
                foreach (var header in headers)
                {
                    request.SetRequestHeader(header.Key, header.Value);
                }
            }

            //recommended minimum of (1024*1024) 1024 Kb
            ulong loadedBytes = 1024 * (ulong)minimumKbForStreaming;
            if (minimumKbForStreaming <= 0)
            {
                enableStreaming = false;
            }

            if (enableStreaming)
            {
                var downloadHandler = ((DownloadHandlerAudioClip)request.downloadHandler);
                downloadHandler.streamAudio = true;
            }

            var asyncRequest = request.SendWebRequest();
            asyncRequest.completed += (x) =>
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (!string.IsNullOrEmpty(request.error))
                {
                    taskSource.TrySetResult(false);
                    return;
                }

                if (x.isDone)
                {
                    taskSource.TrySetResult(x.isDone);
                }
            };
            if (enableStreaming)
            {
                while (!token.IsCancellationRequested && string.IsNullOrEmpty(request.error) &&
                       request.downloadedBytes < loadedBytes && !request.isDone)
                {
                    await Task.Delay(300);
                }

                taskSource.TrySetResult(true);
            }

            await taskSource.Task;
            return request;
        }
    }
}