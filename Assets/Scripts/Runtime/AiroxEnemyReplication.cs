using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;
using Airox.Client.Networking;

namespace Airox.Client.Runtime
{
    public sealed class AiroxEnemyReplication : MonoBehaviour
    {
        [SerializeField] private AiroxUnityRealtimeClient realtime;
        [SerializeField] private Transform localPlayer;
        [SerializeField] private float interpolation = 14f;
        private readonly Dictionary<string, RemotePlayer> players = new Dictionary<string, RemotePlayer>(StringComparer.Ordinal);
        private readonly Dictionary<string, Vector3> targets = new Dictionary<string, Vector3>(StringComparer.Ordinal);
        [SerializeField] private float snapshotIntervalFallback = 0.05f;
        [SerializeField] private float interpolationBackTime = 0.10f;
        private double estimatedServerTime;

        private sealed class RemotePlayer
        {
            public GameObject Root;
            public Renderer Renderer;
            public float Health = 100f;
            public bool Dead;
            public string WeaponId;
            public GameObject WeaponVisual;
            public AiroxRemoteSnapshotBuffer Buffer = new AiroxRemoteSnapshotBuffer(8);
        }

        private void Awake()
        {
            if (realtime == null) realtime = FindObjectOfType<AiroxUnityRealtimeClient>();
            if (localPlayer == null)
            {
                var local = GameObject.Find("LocalPlayer_ServerDriven");
                if (local != null) localPlayer = local.transform;
            }
            if (realtime != null) realtime.SnapshotReceived += OnSnapshot;
        }

        private void Update()
        {
            estimatedServerTime += Time.deltaTime;
            var renderTime = estimatedServerTime - interpolationBackTime;
            foreach (var pair in players)
            {
                if (pair.Value.Root == null) continue;
                var target = pair.Value.Buffer.Evaluate(renderTime, pair.Value.Root.transform.position);
                targets[pair.Key] = target;
                pair.Value.Root.transform.position = Vector3.Lerp(pair.Value.Root.transform.position, target, 1f - Mathf.Exp(-interpolation * Time.deltaTime));
                if (pair.Value.Root.activeSelf != !pair.Value.Dead) pair.Value.Root.SetActive(!pair.Value.Dead);
            }
        }

        private void OnSnapshot(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;
            var playerArray = ExtractArray(json, "players");
            if (string.IsNullOrEmpty(playerArray)) playerArray = ExtractArray(json, "playerStates");
            if (string.IsNullOrEmpty(playerArray)) return;

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var obj in SplitObjects(playerArray))
            {
                var id = StringValue(obj, "playerId");
                if (string.IsNullOrWhiteSpace(id)) id = StringValue(obj, "id");
                if (string.IsNullOrWhiteSpace(id)) continue;
                if (localPlayer != null && string.Equals(id, localPlayer.name, StringComparison.OrdinalIgnoreCase)) continue;
                var x = Number(obj, "x", 0f); var y = Number(obj, "y", 0f); var z = Number(obj, "z", 0f);
                var hp = Number(obj, "health", 100f);
                var dead = BoolValue(obj, "dead") || hp <= 0.01f;
                var weaponId = StringValue(obj, "equippedWeapon");
                if (string.IsNullOrEmpty(weaponId)) weaponId = StringValue(obj, "weaponId");
                seen.Add(id);
                if (!players.TryGetValue(id, out var remote)) remote = CreateRemote(id);
                var position = new Vector3(x, y, z);
                var snapshotTime = Number(json, "serverTime", Number(json, "timestamp", 0f));
                if (snapshotTime <= 0f) snapshotTime = (float)estimatedServerTime + snapshotIntervalFallback;
                remote.Buffer.Add(snapshotTime, position);
                targets[id] = position;
                estimatedServerTime = Math.Max(estimatedServerTime, snapshotTime);
                remote.Health = hp; remote.Dead = dead; remote.WeaponId = weaponId ?? remote.WeaponId;
                UpdateVisual(remote, hp, dead);
                UpdateWeaponVisual(remote);
            }

            var stale = new List<string>();
            foreach (var id in players.Keys) if (!seen.Contains(id)) stale.Add(id);
            foreach (var id in stale) Remove(id);
        }

        private RemotePlayer CreateRemote(string id)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            root.name = "RemotePlayer_" + id;
            root.transform.localScale = new Vector3(0.85f, 1.05f, 0.85f);
            var remote = new RemotePlayer { Root = root, Renderer = root.GetComponent<Renderer>() };
            players[id] = remote;
            return remote;
        }

        private static void UpdateVisual(RemotePlayer remote, float hp, bool dead)
        {
            if (remote.Renderer == null) return;
            var material = remote.Renderer.material;
            material.color = dead ? Color.gray : (hp < 35f ? Color.red : Color.white);
        }

        private static void UpdateWeaponVisual(RemotePlayer remote)
        {
            if (remote.Root == null) return;
            if (remote.WeaponVisual == null)
            {
                remote.WeaponVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                remote.WeaponVisual.name = "RemoteWeapon";
                remote.WeaponVisual.transform.SetParent(remote.Root.transform, false);
                remote.WeaponVisual.transform.localPosition = new Vector3(.42f, .55f, .45f);
                remote.WeaponVisual.transform.localScale = new Vector3(.08f, .08f, .65f);
                var collider = remote.WeaponVisual.GetComponent<Collider>();
                if (collider != null) Destroy(collider);
            }
            remote.WeaponVisual.SetActive(!remote.Dead && !string.IsNullOrEmpty(remote.WeaponId));
            if (remote.WeaponVisual.activeSelf)
            {
                remote.WeaponVisual.transform.localScale = remote.WeaponId == "shotgun" ? new Vector3(.14f, .12f, .58f) : new Vector3(.07f, .07f, .72f);
            }
        }

        private void Remove(string id)
        {
            if (players.TryGetValue(id, out var remote) && remote.Root != null) Destroy(remote.Root);
            players.Remove(id); targets.Remove(id);
        }

        private void OnDestroy()
        {
            if (realtime != null) realtime.SnapshotReceived -= OnSnapshot;
            foreach (var id in new List<string>(players.Keys)) Remove(id);
        }

        private static IEnumerable<string> SplitObjects(string array)
        {
            int depth = 0, start = -1; bool quoted = false, escaped = false;
            for (int i = 0; i < array.Length; i++)
            {
                char c = array[i];
                if (quoted) { if (escaped) escaped = false; else if (c == '\\') escaped = true; else if (c == '"') quoted = false; continue; }
                if (c == '"') { quoted = true; continue; }
                if (c == '{') { if (depth == 0) start = i; depth++; }
                else if (c == '}') { depth--; if (depth == 0 && start >= 0) { yield return array.Substring(start, i - start + 1); start = -1; } }
            }
        }

        private static string ExtractArray(string json, string key)
        {
            var match = Regex.Match(json, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\[");
            if (!match.Success) return null;
            int start = match.Index + match.Length - 1, depth = 0; bool quoted = false, escaped = false;
            for (int i = start; i < json.Length; i++)
            {
                char c = json[i];
                if (quoted) { if (escaped) escaped = false; else if (c == '\\') escaped = true; else if (c == '"') quoted = false; continue; }
                if (c == '"') { quoted = true; continue; }
                if (c == '[') depth++; else if (c == ']' && --depth == 0) return json.Substring(start + 1, i - start - 1);
            }
            return null;
        }

        private static float Number(string s, string key, float fallback)
        {
            var m = Regex.Match(s, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(-?\\d+(?:\\.\\d+)?)", RegexOptions.IgnoreCase);
            return m.Success && float.TryParse(m.Groups[1].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : fallback;
        }
        private static string StringValue(string s, string key)
        {
            var m = Regex.Match(s, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*\\\"([^\\\"]*)\\\"", RegexOptions.IgnoreCase);
            return m.Success ? m.Groups[1].Value : null;
        }
        private static bool BoolValue(string s, string key)
        {
            var m = Regex.Match(s, "\\\"" + Regex.Escape(key) + "\\\"\\s*:\\s*(true|false)", RegexOptions.IgnoreCase);
            return m.Success && string.Equals(m.Groups[1].Value, "true", StringComparison.OrdinalIgnoreCase);
        }
    }
}
