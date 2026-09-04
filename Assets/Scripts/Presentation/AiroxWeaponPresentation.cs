using System;
using UnityEngine;
using Airox.Client.Combat;
using Airox.Client.Networking;
using Airox.Client.Runtime;

namespace Airox.Client.Presentation
{
    public sealed class AiroxWeaponPresentation : MonoBehaviour
    {
        [SerializeField] private AiroxMobileCombatController combat;
        [SerializeField] private AiroxUnityRealtimeClient realtime;
        [SerializeField] private AiroxSnapshotRuntimeDriver runtime;
        [SerializeField] private Camera aimCamera;
        private float muzzleUntil;
        private float hitMarkerUntil;
        private float damageUntil;
        private float eliminationUntil;
        private Vector3 cameraKick;
        private float lastHealth = 100f;
        private GameObject muzzle;
        private GUIStyle labelStyle;

        private void Awake()
        {
            if (combat == null) combat = FindObjectOfType<AiroxMobileCombatController>();
            if (realtime == null) realtime = FindObjectOfType<AiroxUnityRealtimeClient>();
            if (runtime == null) runtime = FindObjectOfType<AiroxSnapshotRuntimeDriver>();
            if (aimCamera == null) aimCamera = Camera.main;
            if (combat != null) combat.Fired += OnFired;
            if (realtime != null) realtime.CombatAcknowledged += OnCombatAck;
            if (runtime != null) runtime.HealthChanged += OnHealthChanged;
            lastHealth = runtime != null ? runtime.Health : 100f;
            CreateMuzzle();
        }

        private void CreateMuzzle()
        {
            var local = GameObject.Find("LocalPlayer_ServerDriven");
            if (local == null) return;
            muzzle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            muzzle.name = "Weapon_MuzzleFlash";
            muzzle.transform.SetParent(local.transform, false);
            muzzle.transform.localPosition = new Vector3(0.35f, 1.05f, 0.75f);
            muzzle.transform.localScale = Vector3.one * 0.12f;
            var collider = muzzle.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            muzzle.SetActive(false);
        }

        private void OnFired(string weaponId)
        {
            muzzleUntil = Time.unscaledTime + 0.055f;
            cameraKick += new Vector3(-1.7f, 0.25f, 0f);
        }

        private void OnCombatAck(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            var hit = message.IndexOf("hit", StringComparison.OrdinalIgnoreCase) >= 0 || message.IndexOf("damage", StringComparison.OrdinalIgnoreCase) >= 0;
            if (hit) hitMarkerUntil = Time.unscaledTime + 0.16f;
        }

        private void OnHealthChanged(float health)
        {
            if (health < lastHealth - 0.01f) damageUntil = Time.unscaledTime + 0.18f;
            if (health <= 0.01f) eliminationUntil = Time.unscaledTime + 5f;
            lastHealth = health;
        }

        private void Update()
        {
            if (muzzle != null) muzzle.SetActive(Time.unscaledTime < muzzleUntil);
            if (aimCamera != null && cameraKick.sqrMagnitude > 0.0001f)
            {
                var step = Vector3.Lerp(Vector3.zero, cameraKick, 1f - Mathf.Exp(-22f * Time.unscaledDeltaTime));
                aimCamera.transform.localRotation *= Quaternion.Euler(step);
                cameraKick = Vector3.Lerp(cameraKick, Vector3.zero, 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime));
            }
        }

        private void OnGUI()
        {
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = Mathf.Max(18, Screen.height / 38), fontStyle = FontStyle.Bold };
            }
            if (Time.unscaledTime < hitMarkerUntil)
            {
                GUI.color = Color.white;
                var cx = Screen.width * 0.5f; var cy = Screen.height * 0.5f; const float s = 24f;
                GUI.DrawTexture(new Rect(cx - s, cy - 2, 12, 3), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx + s - 12, cy - 2, 12, 3), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - 2, cy - s, 3, 12), Texture2D.whiteTexture);
                GUI.DrawTexture(new Rect(cx - 2, cy + s - 12, 3, 12), Texture2D.whiteTexture);
            }
            if (Time.unscaledTime < damageUntil)
            {
                GUI.color = new Color(1f, 0f, 0f, 0.16f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            }
            if (Time.unscaledTime < eliminationUntil)
            {
                GUI.color = Color.white;
                GUI.Label(new Rect(0, Screen.height * 0.42f, Screen.width, 60f), "ELIMINATED", labelStyle);
            }
            GUI.color = Color.white;
        }

        private void OnDestroy()
        {
            if (combat != null) combat.Fired -= OnFired;
            if (realtime != null) realtime.CombatAcknowledged -= OnCombatAck;
            if (runtime != null) runtime.HealthChanged -= OnHealthChanged;
            if (muzzle != null) Destroy(muzzle);
        }
    }
}
