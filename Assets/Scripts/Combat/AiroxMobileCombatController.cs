using System;
using UnityEngine;
using Airox.Client.Core;
using Airox.Client.Networking;
using Airox.Client.Runtime;

namespace Airox.Client.Combat
{
    public sealed class AiroxMobileCombatController : MonoBehaviour
    {
        [SerializeField] private AiroxUnityRealtimeClient realtime;
        [SerializeField] private Camera aimCamera;
        [SerializeField] private AiroxClientConfig config;
        [SerializeField] private string weaponId = "sidearm";
        [SerializeField] private string targetPlayerId = "";
        [SerializeField] private float fireInterval = 0.35f;
        private float nextFire;
        private bool fireHeld;

        public event Action<string> Fired;
        public string WeaponId => weaponId;
        public void SetFireHeld(bool held) => fireHeld = held;
        public void FireOnce() => TryFire();
        public void Reload() { if (realtime != null && realtime.IsConnected) _ = realtime.SendReload(weaponId); }
        public void SelectWeapon(string id) { if (string.IsNullOrWhiteSpace(id) || id == weaponId) return; weaponId = id; if (realtime != null && realtime.IsConnected) _ = realtime.SendWeaponSwitch(id); }
        private void Awake()
        {
            if (realtime == null) realtime = FindObjectOfType<AiroxUnityRealtimeClient>();
            if (aimCamera == null) aimCamera = Camera.main;
            if (config != null && string.IsNullOrWhiteSpace(targetPlayerId)) targetPlayerId = config.defaultTargetPlayerId;
        }
        private void Update() { if (fireHeld) TryFire(); }
        private void TryFire()
        {
            if (Time.unscaledTime < nextFire || realtime == null || !realtime.IsConnected || string.IsNullOrWhiteSpace(targetPlayerId)) return;
            nextFire = Time.unscaledTime + Mathf.Max(0.05f, fireInterval);
            var aim = aimCamera != null ? aimCamera.transform.forward : transform.forward;
            Fired?.Invoke(weaponId);
            _ = realtime.SendAttack(targetPlayerId, weaponId, aim);
        }
    }
}
