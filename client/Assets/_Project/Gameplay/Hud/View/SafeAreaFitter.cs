using UnityEngine;

namespace PlowParty.Gameplay.Hud.View
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform _rect;
        private Rect _applied;
        private Vector2Int _appliedScreen;

        private void Awake()
        {
            _rect = (RectTransform)transform;
        }

        private void Update()
        {
            var safeArea = Screen.safeArea;
            var screen = new Vector2Int(Screen.width, Screen.height);
            if (safeArea == _applied && screen == _appliedScreen)
            {
                return;
            }

            _applied = safeArea;
            _appliedScreen = screen;
            _rect.anchorMin = Normalised(new Vector2(safeArea.xMin, safeArea.yMin), screen);
            _rect.anchorMax = Normalised(new Vector2(safeArea.xMax, safeArea.yMax), screen);
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }

        private static Vector2 Normalised(Vector2 point, Vector2Int screen)
        {
            return new Vector2(Mathf.Clamp01(point.x / screen.x), Mathf.Clamp01(point.y / screen.y));
        }
    }
}
