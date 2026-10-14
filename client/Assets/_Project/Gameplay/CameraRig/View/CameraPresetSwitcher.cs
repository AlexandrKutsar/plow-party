using UnityEngine;
using UnityEngine.InputSystem;

namespace PlowParty.Gameplay.CameraRig.View
{
    public sealed class CameraPresetSwitcher : MonoBehaviour
    {
        [SerializeField] private CameraDirector _director;
        [SerializeField] private Key _cycleKey = Key.C;

        private void Awake()
        {
            if (!Application.isEditor)
            {
                Destroy(this);
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard[_cycleKey].wasPressedThisFrame)
            {
                _director.CyclePreset();
            }
        }
    }
}
