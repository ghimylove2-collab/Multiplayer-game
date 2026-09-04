using UnityEngine;

namespace Airox.Client.Runtime
{
    /// <summary>Third-person mobile camera. Right-side touch controls yaw/pitch; no gameplay authority lives here.</summary>
    public sealed class AiroxMobileLookCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float distance = 5.5f;
        [SerializeField] private float height = 2.6f;
        [SerializeField] private float sensitivity = 0.12f;
        [SerializeField] private float minPitch = -15f;
        [SerializeField] private float maxPitch = 55f;
        [SerializeField] private float followSharpness = 18f;
        private float yaw;
        private float pitch = 14f;

        public void Bind(Transform player)
        {
            target = player;
            if (target != null) yaw = target.eulerAngles.y;
            Snap();
        }

        private void LateUpdate()
        {
            if (target == null) return;
            var look = AiroxMobileInput.Look;
            yaw += look.x * sensitivity;
            pitch = Mathf.Clamp(pitch - look.y * sensitivity, minPitch, maxPitch);
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            var desired = target.position + Vector3.up * height - rotation * Vector3.forward * distance;
            transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followSharpness * Time.deltaTime));
            transform.rotation = rotation;
        }

        private void Snap()
        {
            if (target == null) return;
            var rotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = target.position + Vector3.up * height - rotation * Vector3.forward * distance;
            transform.rotation = rotation;
        }
    }
}
