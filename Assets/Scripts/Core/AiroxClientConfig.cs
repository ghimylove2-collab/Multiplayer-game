using UnityEngine;

namespace Airox.Client.Core
{
    [CreateAssetMenu(fileName = "AiroxClientConfig", menuName = "Airox/Client Config")]
    public sealed class AiroxClientConfig : ScriptableObject
    {
        public string apiBaseUrl = "http://10.0.2.2:5000";
        public string websocketBaseUrl = "ws://10.0.2.2:5000";
        [TextArea] public string accessToken = "";
        public string matchId = "";
        public string defaultTargetPlayerId = "";
    }
}
