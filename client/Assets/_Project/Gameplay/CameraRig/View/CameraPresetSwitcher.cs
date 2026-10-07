using UnityEngine;
using UnityEngine.InputSystem;

namespace PlowParty.Gameplay.CameraRig.View
{
    public sealed class CameraPresetSwitcher : MonoBehaviour
    {
        [SerializeField] private CameraDirector _director;
        [SerializeField] private Key _cycleKey = Key.C;
        [SerializeField, Range(0.02f, 0.2f)] private float _buttonHeight = 0.06f;
        [SerializeField, Min(1f)] private float _buttonAspect = 4f;
        [SerializeField, Range(0f, 0.1f)] private float _margin = 0.02f;

        private string _label;
        private int _labelPreset = -1;

        private void Awake()
        {
            if (!Debug.isDebugBuild)
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

        private void OnGUI()
        {
            var height = Screen.height * _buttonHeight;
            var width = height * _buttonAspect;
            var margin = Screen.height * _margin;
            var rect = new Rect(Screen.width - width - margin, margin, width, height);
            if (GUI.Button(rect, Label()))
            {
                _director.CyclePreset();
            }
        }

        private string Label()
        {
            var preset = (int)_director.ActivePreset;
            if (preset != _labelPreset)
            {
                _labelPreset = preset;
                _label = $"Camera: {_director.ActivePreset} [{_cycleKey}]";
            }

            return _label;
        }
    }
}
