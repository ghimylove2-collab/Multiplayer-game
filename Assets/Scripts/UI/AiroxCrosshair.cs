using UnityEngine;

namespace Airox.Client.UI
{
    public sealed class AiroxCrosshair : MonoBehaviour
    {
        [SerializeField] private float size = 18f;
        [SerializeField] private float gap = 6f;
        [SerializeField] private float thickness = 2f;
        private Texture2D pixel;
        private void Awake() { pixel = Texture2D.whiteTexture; }
        private void OnGUI()
        {
            float cx = Screen.width * 0.5f, cy = Screen.height * 0.5f;
            GUI.color = Color.white;
            Draw(new Rect(cx - thickness * 0.5f, cy - gap - size, thickness, size));
            Draw(new Rect(cx - thickness * 0.5f, cy + gap, thickness, size));
            Draw(new Rect(cx - gap - size, cy - thickness * 0.5f, size, thickness));
            Draw(new Rect(cx + gap, cy - thickness * 0.5f, size, thickness));
        }
        private void Draw(Rect rect) { GUI.DrawTexture(rect, pixel); }
    }
}
