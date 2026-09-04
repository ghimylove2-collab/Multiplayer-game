using System;
using System.Collections.Generic;
using UnityEngine;

namespace Airox.Client.Runtime
{
    /// <summary>
    /// Lightweight client prediction/reconciliation buffer. The server remains authoritative.
    /// It predicts only the local visual transform and replays unacknowledged inputs after an authoritative snapshot.
    /// </summary>
    public sealed class AiroxClientMovementReconciliation : MonoBehaviour
    {
        [Serializable]
        private struct PendingInput
        {
            public int Sequence;
            public Vector2 Move;
            public bool Sprint;
            public float DeltaTime;
        }

        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float sprintMultiplier = 1.35f;
        [SerializeField] private float hardSnapDistance = 2.5f;
        [SerializeField] private float softCorrectionSpeed = 14f;
        [SerializeField] private int maxPendingInputs = 64;

        private readonly Queue<PendingInput> pending = new Queue<PendingInput>();
        private Vector3 authoritativePosition;
        private bool hasAuthoritativePosition;
        private int lastAcknowledgedSequence;

        public int LastAcknowledgedSequence => lastAcknowledgedSequence;
        public int PendingCount => pending.Count;

        public void RecordPredictedInput(int sequence, Vector2 move, bool sprint, float deltaTime)
        {
            move = Vector2.ClampMagnitude(move, 1f);
            var input = new PendingInput
            {
                Sequence = sequence,
                Move = move,
                Sprint = sprint,
                DeltaTime = Mathf.Clamp(deltaTime, 0f, 0.1f)
            };
            pending.Enqueue(input);
            while (pending.Count > Mathf.Max(8, maxPendingInputs)) pending.Dequeue();
        }

        public void ApplyAuthoritativeSnapshot(Vector3 serverPosition, int acknowledgedSequence)
        {
            authoritativePosition = serverPosition;
            hasAuthoritativePosition = true;
            if (acknowledgedSequence > lastAcknowledgedSequence)
                lastAcknowledgedSequence = acknowledgedSequence;

            while (pending.Count > 0 && pending.Peek().Sequence <= lastAcknowledgedSequence)
                pending.Dequeue();

            var replayed = serverPosition;
            foreach (var input in pending)
                replayed += PredictDelta(input);

            var error = replayed - transform.position;
            if (error.sqrMagnitude >= hardSnapDistance * hardSnapDistance)
                transform.position = replayed;
            else if (error.sqrMagnitude > 0.0001f)
                transform.position = Vector3.MoveTowards(transform.position, replayed, softCorrectionSpeed * Time.deltaTime);
        }

        public void ReconcileWithoutAck(Vector3 serverPosition)
        {
            authoritativePosition = serverPosition;
            hasAuthoritativePosition = true;
            var error = serverPosition - transform.position;
            if (error.sqrMagnitude >= hardSnapDistance * hardSnapDistance)
                transform.position = serverPosition;
            else if (error.sqrMagnitude > 0.0001f)
                transform.position = Vector3.MoveTowards(transform.position, serverPosition, softCorrectionSpeed * Time.deltaTime);
        }

        private Vector3 PredictDelta(PendingInput input)
        {
            var speed = input.Sprint ? walkSpeed * sprintMultiplier : walkSpeed;
            return new Vector3(input.Move.x, 0f, input.Move.y) * speed * input.DeltaTime;
        }

        private void LateUpdate()
        {
            if (!hasAuthoritativePosition || pending.Count == 0) return;
            var target = authoritativePosition;
            foreach (var input in pending) target += PredictDelta(input);
            var correction = target - transform.position;
            if (correction.sqrMagnitude < hardSnapDistance * hardSnapDistance)
                transform.position = Vector3.MoveTowards(transform.position, target, softCorrectionSpeed * Time.deltaTime);
        }
    }
}
