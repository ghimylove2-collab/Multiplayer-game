using UnityEngine;

namespace Airox.Client.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class AiroxThirdPersonController : MonoBehaviour
    {
        public float moveSpeed = 5f;
        public float sprintSpeed = 7.5f;
        public float gravity = -20f;
        public float jumpHeight = 1.2f;
        private CharacterController controller;
        private Vector3 velocity;
        private void Awake() => controller = GetComponent<CharacterController>();
        private void Update()
        {
            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            input = Vector2.ClampMagnitude(input, 1f);
            var move = new Vector3(input.x, 0, input.y);
            var speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
            controller.Move(move * speed * Time.deltaTime);
            if (controller.isGrounded && velocity.y < 0) velocity.y = -2f;
            if (controller.isGrounded && Input.GetButtonDown("Jump")) velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
