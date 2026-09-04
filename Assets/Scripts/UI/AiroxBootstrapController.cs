using UnityEngine;
using UnityEngine.SceneManagement;
using Airox.Client.Networking;

namespace Airox.Client.UI
{
    public sealed class AiroxBootstrapController : MonoBehaviour
    {
        [SerializeField] private AiroxUnityRealtimeClient realtime;
        private void Start()
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            if (realtime != null) realtime.StatusChanged += OnStatus;
        }
        public void OpenBattleRoyale() => SceneManager.LoadScene("BR_Prototype");
        public void Connect() { if (realtime != null) realtime.Connect(); }
        private void OnStatus(string status) => Debug.Log("[Airox] " + status);
        private void OnDestroy() { if (realtime != null) realtime.StatusChanged -= OnStatus; }
    }
}
