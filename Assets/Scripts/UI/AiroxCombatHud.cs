using UnityEngine;
using Airox.Client.Combat;
using Airox.Client.Runtime;

namespace Airox.Client.UI
{
    public sealed class AiroxCombatHud : MonoBehaviour
    {
        [SerializeField] private AiroxMobileCombatController combat;
        [SerializeField] private AiroxSnapshotRuntimeDriver runtime;
        private bool firing;
        public void Bind(AiroxMobileCombatController c, AiroxSnapshotRuntimeDriver r) { combat = c; runtime = r; }
        private void OnGUI()
        {
            if (combat == null) return;
            float w = Mathf.Clamp(Screen.width * 0.22f, 170f, 300f);
            float h = Mathf.Clamp(Screen.height * 0.09f, 64f, 92f);
            var fireRect = new Rect(Screen.width - w - 24, Screen.height - h - 34, w, h);
            var reloadRect = new Rect(Screen.width - w - 24, Screen.height - h * 2 - 46, w * 0.48f, h * 0.72f);
            var switchRect = new Rect(Screen.width - w * 0.48f - 34, Screen.height - h * 2 - 46, w * 0.48f, h * 0.72f);
            var jumpRect = new Rect(24, Screen.height - h - 34, w * 0.42f, h * 0.72f);
            if (GUI.Button(fireRect, firing ? "FIRING" : "FIRE")) { firing = !firing; combat.SetFireHeld(firing); if (firing) combat.FireOnce(); }
            if (GUI.Button(reloadRect, "RELOAD")) combat.Reload();
            if (GUI.Button(switchRect, "SWITCH")) combat.SelectWeapon(combat.WeaponId == "sidearm" ? "rifle" : "sidearm");
            if (GUI.Button(jumpRect, "JUMP")) AiroxMobileInput.RequestJump();
            if (runtime != null) GUI.Label(new Rect(Screen.width - w - 24, 20, w, 34), $"WEAPON  {combat.WeaponId}");
        }
        private void OnDisable() { if (combat != null) combat.SetFireHeld(false); }
    }
}
