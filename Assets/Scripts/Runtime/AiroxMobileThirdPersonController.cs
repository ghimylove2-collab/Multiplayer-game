using System.Threading.Tasks;
using UnityEngine;
using Airox.Client.Networking;

namespace Airox.Client.Runtime
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class AiroxMobileThirdPersonController : MonoBehaviour
    {
        public float moveSpeed = 4.5f;
        private CharacterController cc; private AiroxUnityRealtimeClient net; private Vector3 velocity; private int sendFrame; private AiroxClientMovementReconciliation reconciliation;
        private void Awake() { cc = GetComponent<CharacterController>(); net = FindObjectOfType<AiroxUnityRealtimeClient>(); reconciliation = GetComponent<AiroxClientMovementReconciliation>() ?? gameObject.AddComponent<AiroxClientMovementReconciliation>(); }
        private void Update()
        {
            var input = AiroxMobileInput.Move; if (input.sqrMagnitude < 0.001f) input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1);
            // Do not move Transform directly here. Prediction is handled by
            // AiroxClientMovementReconciliation while the server remains authoritative.
            if (++sendFrame % 3 == 0 && net != null && net.IsConnected) _ = Send(input);
        }
        private async Task Send(Vector2 input) { var sprint = AiroxMobileInput.Sprint; var jump = AiroxMobileInput.ConsumeJump(); var sequence = net.ReserveInputSequence(); reconciliation.RecordPredictedInput(sequence, input, sprint, Time.deltaTime * 3f); await net.SendInputWithSequence(sequence, input.x, input.y, sprint, jump); }
    }
}
