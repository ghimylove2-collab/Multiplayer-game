using UnityEngine;

namespace Airox.Client.Runtime
{
    public static class AiroxVisualRuntimeGuardBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            GameObject existing = GameObject.Find("Airox_VisualRuntimeGuard");

            if (existing == null)
            {
                existing = new GameObject("Airox_VisualRuntimeGuard");
                existing.AddComponent<AiroxVisualRuntimeGuard>();
            }
        }
    }
}
