using UnityEngine;

namespace Airox.Client.Runtime
{
    public sealed class AiroxRuntimeHud : MonoBehaviour
    {
        private AiroxSnapshotRuntimeDriver driver; public string Status = "Offline";
        public void Bind(AiroxSnapshotRuntimeDriver d) => driver = d;
        private void OnGUI()
        {
            if (driver == null) return;
            GUI.Label(new Rect(20, 18, 500, 32), $"AIROX BR  |  {Status}  |  {driver.Phase}");
            GUI.Label(new Rect(20, 52, 500, 32), $"HP {driver.Health:0}   ARM {driver.Armor:0}   AMMO {driver.Ammo:0}/{driver.Reserve:0}");
            GUI.Label(new Rect(20, 86, 500, 32), $"SAFE ZONE  R={driver.ZoneRadius:0}");
            GUI.Label(new Rect(20, Screen.height - 70, 520, 40), "Touch: left = move   |   right = jump/aim input");
        }
    }
}
