using UnityEngine;

namespace Airox.Client.Runtime
{
    public sealed class AiroxMobileInput : MonoBehaviour
    {
        public static Vector2 Move { get; private set; }
        public static Vector2 Look { get; private set; }
        public static bool Sprint { get; private set; }
        private static bool jump;
        private int moveFinger = -1;
        private int lookFinger = -1;
        private Vector2 moveOrigin;
        private Vector2 lastLookPosition;

        public static bool ConsumeJump() { var v = jump; jump = false; return v; }
        public static void RequestJump() => jump = true;

        private void Update()
        {
            Move = Vector2.zero;
            Look = Vector2.zero;
            Sprint = false;
            for (int i = 0; i < Input.touchCount; i++)
            {
                var touch = Input.GetTouch(i);
                if (touch.position.x < Screen.width * .5f) HandleMove(touch);
                else HandleLook(touch);
            }
            if (Input.GetKey(KeyCode.LeftShift)) Sprint = true;
            if (Input.GetButtonDown("Jump")) jump = true;
        }

        private void HandleMove(Touch touch)
        {
            if (touch.phase == TouchPhase.Began && moveFinger < 0)
            {
                moveFinger = touch.fingerId;
                moveOrigin = touch.position;
            }
            if (touch.fingerId != moveFinger) return;
            if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                var delta = (touch.position - moveOrigin) / (Screen.height * .16f);
                Move = Vector2.ClampMagnitude(delta, 1f);
                Sprint = Move.magnitude > .82f;
            }
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) moveFinger = -1;
        }

        private void HandleLook(Touch touch)
        {
            if (touch.phase == TouchPhase.Began && lookFinger < 0)
            {
                lookFinger = touch.fingerId;
                lastLookPosition = touch.position;
                return;
            }
            if (touch.fingerId != lookFinger) return;
            if (touch.phase == TouchPhase.Moved)
            {
                Look = touch.position - lastLookPosition;
                lastLookPosition = touch.position;
            }
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) lookFinger = -1;
        }
    }
}
