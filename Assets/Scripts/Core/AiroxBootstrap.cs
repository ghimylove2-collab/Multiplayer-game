using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airox.Client.Core
{
    public sealed class AiroxBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Configure() => Application.targetFrameRate = 60;
        private void Start()
        {
            if (SceneManager.GetActiveScene().name == "Bootstrap")
                SceneManager.LoadScene("BR_Prototype");
        }
    }
}
