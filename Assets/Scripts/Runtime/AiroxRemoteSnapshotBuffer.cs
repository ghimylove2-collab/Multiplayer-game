using System;
using System.Collections.Generic;
using UnityEngine;

namespace Airox.Client.Runtime
{
    public sealed class AiroxRemoteSnapshotBuffer
    {
        public readonly struct Sample
        {
            public readonly double Time;
            public readonly Vector3 Position;
            public Sample(double time, Vector3 position) { Time = time; Position = position; }
        }

        private readonly int capacity;
        private readonly List<Sample> samples = new List<Sample>(8);
        public AiroxRemoteSnapshotBuffer(int capacity = 8) { this.capacity = Mathf.Clamp(capacity, 2, 32); }
        public void Add(double serverTime, Vector3 position)
        {
            if (samples.Count > 0 && serverTime <= samples[samples.Count - 1].Time) return;
            samples.Add(new Sample(serverTime, position));
            if (samples.Count > capacity) samples.RemoveAt(0);
        }
        public Vector3 Evaluate(double renderTime, Vector3 fallback)
        {
            if (samples.Count == 0) return fallback;
            if (samples.Count == 1) return samples[0].Position;
            if (renderTime <= samples[0].Time) return samples[0].Position;
            for (int i = 1; i < samples.Count; i++)
            {
                var b = samples[i]; var a = samples[i - 1];
                if (renderTime <= b.Time)
                {
                    float t = Mathf.Clamp01((float)((renderTime - a.Time) / Math.Max(0.0001, b.Time - a.Time)));
                    return Vector3.LerpUnclamped(a.Position, b.Position, t);
                }
            }
            var last = samples[samples.Count - 1];
            var previous = samples[samples.Count - 2];
            var dt = Math.Max(0.001, last.Time - previous.Time);
            var velocity = (last.Position - previous.Position) / (float)dt;
            var extrapolation = Mathf.Clamp((float)(renderTime - last.Time), 0f, 0.10f);
            return last.Position + velocity * extrapolation;
        }
        public void Clear() => samples.Clear();
    }
}
