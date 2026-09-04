using UnityEngine;

namespace Airox.Client.Networking
{
    [CreateAssetMenu(menuName = "Airox/Networking/Connection Config", fileName = "AiroxConnectionConfig")]
    public sealed class AiroxConnectionConfig : ScriptableObject
    {
        public string apiBaseUrl = "http://10.0.2.2:5000";
        public string websocketBaseUrl = "ws://10.0.2.2:5000";
        public string protocol = "airox.v1";
    }
}
