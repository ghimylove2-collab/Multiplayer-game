using System;
using UnityEngine;
using Airox.Client.Networking;

namespace Airox.Client.UI
{
    public sealed class AiroxCombatFeedback : MonoBehaviour
    {
        [SerializeField] private AiroxUnityRealtimeClient realtime;
        private float hitFlashUntil;
        private string lastAck = "";
        public bool IsHitFlashActive => Time.unscaledTime < hitFlashUntil;
        public string LastAck => lastAck;

        private void Awake()
        {
            if (realtime == null) realtime = FindObjectOfType<AiroxUnityRealtimeClient>();
            if (realtime != null) realtime.CombatAcknowledged += OnAck;
        }
        private void OnAck(string message)
        {
            lastAck = message ?? "";
            if (lastAck.IndexOf("hit", StringComparison.OrdinalIgnoreCase) >= 0 || lastAck.IndexOf("damage", StringComparison.OrdinalIgnoreCase) >= 0)
                hitFlashUntil = Time.unscaledTime + 0.12f;
        }
        private void OnGUI()
        {
            if (!IsHitFlashActive) return;
            GUI.color = new Color(1f, 1f, 1f, 0.12f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
        private void OnDestroy() { if (realtime != null) realtime.CombatAcknowledged -= OnAck; }
    }
}
