using UnityEngine;
using Airox.Client.Core;

namespace Airox.Client.BR
{
    public sealed class BattleRoyaleClientState : MonoBehaviour
    {
        public string MatchId { get; private set; }
        public float SafeZoneX { get; private set; }
        public float SafeZoneZ { get; private set; }
        public float SafeZoneRadius { get; private set; }

        public void ApplySnapshot(string matchId, float x, float z, float radius)
        {
            MatchId = matchId;
            SafeZoneX = x;
            SafeZoneZ = z;
            SafeZoneRadius = Mathf.Max(0f, radius);
        }
    }
}
