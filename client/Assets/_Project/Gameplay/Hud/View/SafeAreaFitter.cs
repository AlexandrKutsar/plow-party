using UnityEngine;

namespace PlowParty.Gameplay.Hud.View
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _applied;

        public static Rect ClampedSafeArea()
        {
            var safeArea = Screen.safeArea;
            var xMin = Mathf.Clamp(safeArea.xMin, 0f, Screen.width);
            var yMin = Mathf.Clamp(safeArea.yMin, 0f, Screen.height);
            var xMax = Mathf.Clamp(safeArea.xMax, xMin, Screen.width);
            var yMax = Mathf.Clamp(safeArea.yMax, yMin, Screen.height);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private void Awake()
        {
            _rect = (RectTransform)transform;
        }

        private void Update()
        {
            var safeArea = ClampedSafeArea();
            if (safeArea == _applied)
            {
                return;
            }

            _applied = safeArea;
            var screen = new Vector2(Screen.width, Screen.height);
            _rect.anchorMin = safeArea.min / screen;
            _rect.anchorMax = safeArea.max / screen;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
