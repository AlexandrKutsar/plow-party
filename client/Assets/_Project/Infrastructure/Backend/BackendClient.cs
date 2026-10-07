using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace PlowParty.Infrastructure.Backend
{
    public sealed class BackendClient
    {
        private const string JsonContentType = "application/json";

        private readonly string _baseUrl;
        private readonly int _timeoutSeconds;

        public BackendClient(BackendConfig config)
        {
            _baseUrl = config.BaseUrlFor(Application.isEditor);
            _timeoutSeconds = config.TimeoutSeconds;
        }

        public async UniTask<BackendResponse> SendAsync(BackendRequest request, CancellationToken cancellationToken)
        {
            using var webRequest = Build(request);
            try
            {
                await webRequest.SendWebRequest().ToUniTask(cancellationToken: cancellationToken);
            }
            catch (UnityWebRequestException)
            {
            }

            var reachedServer = webRequest.result != UnityWebRequest.Result.ConnectionError;
            var outcome = BackendResponse.OutcomeFor(reachedServer, webRequest.responseCode);
            if (outcome != BackendOutcome.Ok)
            {
                Debug.LogWarning($"Backend {request.Method} {request.Path}: {outcome} {webRequest.responseCode} {webRequest.error}");
            }

            return new BackendResponse(outcome, webRequest.responseCode, webRequest.downloadHandler?.text);
        }

        private UnityWebRequest Build(BackendRequest request)
        {
            var webRequest = new UnityWebRequest(_baseUrl + request.Path, request.Method)
            {
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = _timeoutSeconds,
            };
            if (request.Body != null)
            {
                webRequest.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(BackendJson.Serialize(request.Body)));
                webRequest.SetRequestHeader("Content-Type", JsonContentType);
            }

            if (!string.IsNullOrEmpty(request.AuthToken))
            {
                webRequest.SetRequestHeader("Authorization", "Bearer " + request.AuthToken);
            }

            return webRequest;
        }
    }
}
