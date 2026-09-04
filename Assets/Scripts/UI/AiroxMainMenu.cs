using UnityEngine;
using UnityEngine.SceneManagement;

namespace Airox.Client.UI
{
    public sealed class AiroxMainMenu : MonoBehaviour
    {
        public void StartGame() => SceneManager.LoadScene("BR_Prototype");
        public void QuitGame() => Application.Quit();
    }
}
