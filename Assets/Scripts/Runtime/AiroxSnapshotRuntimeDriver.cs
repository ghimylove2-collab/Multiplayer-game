using System;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using Airox.Client.Networking;
using Airox.Client.BR;

namespace Airox.Client.Runtime
{
    public sealed class AiroxSnapshotRuntimeDriver : MonoBehaviour
    {
        [SerializeField] private AiroxUnityRealtimeClient realtime;
        [SerializeField] private BattleRoyaleClientState state;
        private Transform localPlayer;
        private Camera cam;
        private LineRenderer zone;
        private AiroxRuntimeHud hud;
        private AiroxClientMovementReconciliation reconciliation;
        private float health = 100f, armor, ammo = 12, reserve = 84;
        private string phase = "Waiting";
        private float zoneX, zoneZ, zoneRadius = 500f;
        private Material groundMaterial, playerMaterial, accentMaterial, wallMaterial;

        public event Action<float> HealthChanged;
        public float Health => health; public float Armor => armor; public float Ammo => ammo; public float Reserve => reserve;
        public string Phase => phase; public float ZoneRadius => zoneRadius;

        private void Awake()
        {
            if (state == null) state = gameObject.GetComponent<BattleRoyaleClientState>() ?? gameObject.AddComponent<BattleRoyaleClientState>();
            if (realtime == null) realtime = gameObject.GetComponent<AiroxUnityRealtimeClient>() ?? gameObject.AddComponent<AiroxUnityRealtimeClient>();
            BuildWorld();
            hud = gameObject.AddComponent<AiroxRuntimeHud>(); hud.Bind(this);
            gameObject.AddComponent<AiroxCombatRuntimeInstaller>();
            realtime.SnapshotReceived += ApplySnapshot;
            realtime.StatusChanged += OnStatus;
        }

        private void BuildWorld()
        {
            localPlayer = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
            localPlayer.name = "LocalPlayer_ServerDriven";
            localPlayer.position = new Vector3(0f, 1f, 0f);
            localPlayer.gameObject.AddComponent<CharacterController>();
            localPlayer.gameObject.AddComponent<AiroxMobileThirdPersonController>();
            reconciliation = localPlayer.gameObject.AddComponent<AiroxClientMovementReconciliation>();
            cam = new GameObject("BR_Camera").AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.12f, 0.18f, 1f);
            cam.nearClipPlane = 0.05f; cam.farClipPlane = 1200f;
            cam.transform.SetParent(null);
            cam.transform.position = localPlayer.position + new Vector3(0f, 4.5f, -8f);
            cam.transform.LookAt(localPlayer.position + Vector3.up * 1.0f);

            var light = new GameObject("Sun").AddComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.15f; light.shadows = LightShadows.Soft; light.transform.rotation = Quaternion.Euler(50, -30, 0);
            RenderSettings.ambientLight = new Color(0.32f, 0.36f, 0.44f);

            groundMaterial = MakeMaterial(new Color(0.16f, 0.22f, 0.18f));
            playerMaterial = MakeMaterial(new Color(0.12f, 0.55f, 1f));
            accentMaterial = MakeMaterial(new Color(1f, 0.62f, 0.08f));
            wallMaterial = MakeMaterial(new Color(0.24f, 0.28f, 0.36f));
            ApplyMaterial(localPlayer.GetComponent<Renderer>(), playerMaterial);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name = "BR_Ground"; ground.transform.localScale = Vector3.one * 30f; ApplyMaterial(ground.GetComponent<Renderer>(), groundMaterial);
            BuildTrainingLandmarks();
            zone = new GameObject("SafeZone_ServerAuthoritative").AddComponent<LineRenderer>();
            zone.positionCount = 96; zone.loop = true; zone.widthMultiplier = 0.08f; zone.useWorldSpace = true;
            UpdateZone();
        }

        private Material MakeMaterial(Color color)
        {
            var shader = Resources.Load<Shader>("AiroxRuntimeUnlit");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Standard");
            var material = new Material(shader); material.color = color;
            return material;
        }

        private static void ApplyMaterial(Renderer renderer, Material material)
        {
            if (renderer != null && material != null) renderer.material = material;
        }

        private void BuildTrainingLandmarks()
        {
            CreateBlock("NorthGate", new Vector3(0, 1.5f, 70), new Vector3(22, 3, 2), wallMaterial);
            CreateBlock("SouthGate", new Vector3(0, 1.5f, -70), new Vector3(22, 3, 2), wallMaterial);
            CreateBlock("EastGate", new Vector3(70, 1.5f, 0), new Vector3(2, 3, 22), wallMaterial);
            CreateBlock("WestGate", new Vector3(-70, 1.5f, 0), new Vector3(2, 3, 22), wallMaterial);
            CreateBlock("CenterCover", new Vector3(0, 1.25f, 18), new Vector3(5, 2.5f, 2), accentMaterial);
            CreateBlock("LeftCover", new Vector3(-18, 1.25f, 4), new Vector3(2, 2.5f, 7), accentMaterial);
            CreateBlock("RightCover", new Vector3(18, 1.25f, 4), new Vector3(2, 2.5f, 7), accentMaterial);
        }

        private static void CreateBlock(string name, Vector3 position, Vector3 scale, Material material)
        {
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube); block.name = name; block.transform.position = position; block.transform.localScale = scale; ApplyMaterial(block.GetComponent<Renderer>(), material);
        }

        private void LateUpdate()
        {
            if (cam == null || localPlayer == null)
                return;

            Vector3 target = localPlayer.position + Vector3.up * 1.0f;
            Vector3 desired = localPlayer.position + new Vector3(0f, 4.5f, -8f);

            cam.transform.position = Vector3.Lerp(
                cam.transform.position,
                desired,
                1f - Mathf.Exp(-10f * Time.deltaTime)
            );

            cam.transform.LookAt(target);
            cam.fieldOfView = 60f;
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 1000f;
        }

        private void ApplySnapshot(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;
            var x = Number(json, "x", localPlayer.position.x); var y = Number(json, "y", localPlayer.position.y); var z = Number(json, "z", localPlayer.position.z);
            var hp = Number(json, "health", health); var ar = Number(json, "armor", armor);
            var am = Number(json, "ammo", ammo); var res = Number(json, "reserveAmmo", reserve);
            var sx = Number(json, "safeZoneX", Number(json, "centerX", zoneX));
            var sz = Number(json, "safeZoneZ", Number(json, "centerZ", zoneZ));
            var sr = Number(json, "safeZoneRadius", Number(json, "radius", zoneRadius));
            var p = StringValue(json, "phase"); if (!string.IsNullOrEmpty(p)) phase = p;
            var previousHealth = health;
            health = Mathf.Clamp(hp, 0, 100); armor = Mathf.Max(0, ar);
            if (Mathf.Abs(previousHealth - health) > 0.001f) HealthChanged?.Invoke(health); ammo = Mathf.Max(0, am); reserve = Mathf.Max(0, res);
            var serverPosition = new Vector3(x, y, z);
            var acknowledged = Number(json, "inputAckSequence", Number(json, "lastProcessedInputSequence", 0));
            if (reconciliation != null && acknowledged > 0) reconciliation.ApplyAuthoritativeSnapshot(serverPosition, Mathf.RoundToInt(acknowledged));
            else if (reconciliation != null) reconciliation.ReconcileWithoutAck(serverPosition);
            else localPlayer.position = serverPosition;
            zoneX = sx; zoneZ = sz; zoneRadius = Mathf.Max(0, sr);
            state.ApplySnapshot(state.MatchId ?? "", x, z, zoneRadius); UpdateZone();
        }

        private void OnStatus(string status) { if (hud != null) hud.Status = status; }
        private void UpdateZone()
        {
            if (zone == null) return;
            for (int i = 0; i < zone.positionCount; i++) { float a = i * Mathf.PI * 2f / zone.positionCount; zone.SetPosition(i, new Vector3(zoneX + Mathf.Cos(a) * zoneRadius, 0.05f, zoneZ + Mathf.Sin(a) * zoneRadius)); }
        }
        private static float Number(string s, string key, float fallback) { var m = Regex.Match(s, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?)", RegexOptions.IgnoreCase); return m.Success && float.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback; }
        private static string StringValue(string s, string key) { var m = Regex.Match(s, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"", RegexOptions.IgnoreCase); return m.Success ? m.Groups[1].Value : null; }
        private void OnDestroy() { if (realtime != null) { realtime.SnapshotReceived -= ApplySnapshot; realtime.StatusChanged -= OnStatus; } }
    }
}
