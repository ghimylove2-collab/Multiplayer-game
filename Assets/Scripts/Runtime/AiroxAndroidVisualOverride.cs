using UnityEngine;

namespace Airox.Client.Runtime
{
    public sealed class AiroxAndroidVisualOverride : MonoBehaviour
    {
        private Shader runtimeShader;
        private Material playerMaterial;
        private Material groundMaterial;
        private Material accentMaterial;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            GameObject root = new GameObject("Airox_AndroidVisualOverride");
            DontDestroyOnLoad(root);
            root.AddComponent<AiroxAndroidVisualOverride>();
        }

        private void Awake()
        {
            runtimeShader = Resources.Load<Shader>("AiroxRuntimeUnlit");

            if (runtimeShader == null)
            {
                Debug.LogError("[AiroxVisual] AiroxRuntimeUnlit shader was not found.");
                return;
            }

            playerMaterial = CreateMaterial(new Color(0.12f, 0.55f, 1.0f));
            groundMaterial = CreateMaterial(new Color(0.24f, 0.18f, 0.12f));
            accentMaterial = CreateMaterial(new Color(1.0f, 0.55f, 0.08f));
        }

        private Material CreateMaterial(Color color)
        {
            Material material = new Material(runtimeShader);
            material.color = color;
            return material;
        }

        private void LateUpdate()
        {
            FixCameras();
            FixRenderers();
        }

        private void FixCameras()
        {
            Camera[] cameras = FindObjectsOfType<Camera>(true);

            Camera preferred = null;

            foreach (Camera camera in cameras)
            {
                if (camera != null && camera.gameObject.name == "Third Person Camera")
                {
                    preferred = camera;
                    break;
                }
            }

            if (preferred == null && cameras.Length > 0)
                preferred = cameras[0];

            if (preferred == null)
                return;

            foreach (Camera camera in cameras)
            {
                if (camera == null)
                    continue;

                camera.enabled = camera == preferred;

                if (camera == preferred)
                {
                    camera.depth = 100;
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = new Color(0.08f, 0.13f, 0.20f, 1f);
                    camera.nearClipPlane = 0.05f;
                    camera.farClipPlane = 500f;
                    camera.cullingMask = -1;
                }
            }
        }

        private void FixRenderers()
        {
            if (runtimeShader == null)
                return;

            Renderer[] renderers = FindObjectsOfType<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null)
                    continue;

                Material current = renderer.material;

                if (current == null)
                    continue;

                string shaderName = current.shader != null
                    ? current.shader.name
                    : "";

                if (shaderName == "Hidden/InternalErrorShader")
                {
                    renderer.material = ChooseMaterial(renderer.gameObject);
                }
            }
        }

        private Material ChooseMaterial(GameObject target)
        {
            if (target == null)
                return groundMaterial;

            string name = target.name;

            if (name.Contains("Player") ||
                name.Contains("Avatar") ||
                name.Contains("Head") ||
                name.Contains("Torso") ||
                name.Contains("Arm"))
            {
                return playerMaterial;
            }

            if (name.Contains("Cover") ||
                name.Contains("Gate") ||
                name.Contains("Energy"))
            {
                return accentMaterial;
            }

            return groundMaterial;
        }
    }
}
