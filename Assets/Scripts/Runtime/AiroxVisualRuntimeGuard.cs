using UnityEngine;
using System.Collections;

namespace Airox.Client.Runtime
{
    public sealed class AiroxVisualRuntimeGuard : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return null;
            yield return new WaitForSeconds(0.25f);

            FixCameras();
            FixPinkMaterials();
            FixLighting();
        }

        private static void FixCameras()
        {
            Camera[] cameras = Object.FindObjectsOfType<Camera>(true);
            Camera preferred = null;

            foreach (Camera cam in cameras)
            {
                if (cam == null) continue;

                if (cam.name == "Third Person Camera")
                {
                    preferred = cam;
                    break;
                }
            }

            if (preferred == null)
            {
                foreach (Camera cam in cameras)
                {
                    if (cam != null && cam.name == "BR_Camera")
                    {
                        preferred = cam;
                        break;
                    }
                }
            }

            if (preferred == null && cameras.Length > 0)
                preferred = cameras[0];

            if (preferred == null)
                return;

            foreach (Camera cam in cameras)
            {
                if (cam == null) continue;
                cam.enabled = cam == preferred;
            }

            preferred.tag = "MainCamera";
            preferred.clearFlags = CameraClearFlags.SolidColor;
            preferred.backgroundColor = new Color(0.12f, 0.16f, 0.22f, 1f);
            preferred.nearClipPlane = 0.05f;
            preferred.farClipPlane = 1200f;
            preferred.fieldOfView = 62f;
        }

        private static void FixPinkMaterials()
        {
            Shader standard = Shader.Find("Standard");
            Shader unlit = Shader.Find("Unlit/Color");

            if (standard == null && unlit == null)
                return;

            Renderer[] renderers = Object.FindObjectsOfType<Renderer>(true);

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || renderer.sharedMaterial == null)
                    continue;

                Shader shader = renderer.sharedMaterial.shader;

                if (shader == null || shader.name == "Hidden/InternalErrorShader")
                {
                    Material material = new Material(standard != null ? standard : unlit);
                    material.color = Color.white;
                    renderer.material = material;
                }
            }
        }

        private static void FixLighting()
        {
            RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.45f);

            Light[] lights = Object.FindObjectsOfType<Light>(true);

            bool directionalFound = false;

            foreach (Light light in lights)
            {
                if (light == null) continue;

                if (light.type == LightType.Directional)
                {
                    directionalFound = true;
                    light.enabled = true;

                    if (light.intensity < 0.5f)
                        light.intensity = 1.0f;
                }
            }

            if (!directionalFound)
            {
                GameObject sunObject = new GameObject("Airox_Runtime_Sun");
                Light sun = sunObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.1f;
                sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }
        }
    }
}
