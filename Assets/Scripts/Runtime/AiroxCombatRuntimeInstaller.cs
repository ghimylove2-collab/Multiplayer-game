using UnityEngine;
using Airox.Client.Combat;
using Airox.Client.Networking;
using Airox.Client.UI;
using Airox.Client.Presentation;

namespace Airox.Client.Runtime
{
    public sealed class AiroxCombatRuntimeInstaller : MonoBehaviour
    {
        [SerializeField] private AiroxUnityRealtimeClient realtime;
        [SerializeField] private AiroxSnapshotRuntimeDriver runtime;
        private void Awake()
        {
            if (realtime == null) realtime = FindObjectOfType<AiroxUnityRealtimeClient>();
            if (runtime == null) runtime = FindObjectOfType<AiroxSnapshotRuntimeDriver>();
            var combat = gameObject.AddComponent<AiroxMobileCombatController>();
            var hud = gameObject.AddComponent<AiroxCombatHud>();
            hud.Bind(combat, runtime);
            if (FindObjectOfType<AiroxEnemyReplication>() == null) gameObject.AddComponent<AiroxEnemyReplication>();
            if (FindObjectOfType<AiroxCrosshair>() == null) gameObject.AddComponent<AiroxCrosshair>();
            if (FindObjectOfType<AiroxCombatFeedback>() == null) gameObject.AddComponent<AiroxCombatFeedback>();
            if (FindObjectOfType<AiroxWeaponPresentation>() == null) gameObject.AddComponent<AiroxWeaponPresentation>();
            var player = GameObject.Find("LocalPlayer_ServerDriven");
            var camera = Camera.main != null ? Camera.main : FindObjectOfType<Camera>();
            if (camera != null && FindObjectOfType<AiroxMobileLookCamera>() == null)
            {
                var lookCamera = camera.gameObject.AddComponent<AiroxMobileLookCamera>();
                lookCamera.Bind(player != null ? player.transform : transform);
            }
        }
    }
}
