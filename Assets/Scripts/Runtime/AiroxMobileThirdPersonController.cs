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
            input = Vector2.ClampMagnitude(input, 1); var move = new Vector3(input.x, 0, input.y); transform.position += move * moveSpeed * Time.deltaTime;
            if (cc != null) { if (cc.isGrounded && velocity.y < 0) velocity.y = -2; velocity.y += -20f * Time.deltaTime; cc.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime); }
            if (++sendFrame % 3 == 0 && net != null && net.IsConnected) _ = Send(input);
        }
        private async Task Send(Vector2 input) { var sprint = AiroxMobileInput.Sprint; var jump = AiroxMobileInput.ConsumeJump(); var sequence = net.ReserveInputSequence(); reconciliation.RecordPredictedInput(sequence, input, sprint, Time.deltaTime * 3f); await net.SendInputWithSequence(sequence, input.x, input.y, sprint, jump); }
    }
}
