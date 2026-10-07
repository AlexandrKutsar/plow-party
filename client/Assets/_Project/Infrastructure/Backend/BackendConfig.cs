using UnityEngine;

namespace PlowParty.Infrastructure.Backend
{
    [CreateAssetMenu(menuName = "Plow Party/Backend Config", fileName = nameof(BackendConfig))]
    public sealed class BackendConfig : ScriptableObject
    {
        [SerializeField] private string _editorBaseUrl = "http://localhost:8000";
        [SerializeField] private string _deviceBaseUrl = "http://192.168.1.2:8000";
        [SerializeField, Min(1)] private int _timeoutSeconds = 4;

        public int TimeoutSeconds => _timeoutSeconds;

        public string BaseUrlFor(bool isEditor)
        {
            return (isEditor ? _editorBaseUrl : _deviceBaseUrl).TrimEnd('/');
        }
    }
}
