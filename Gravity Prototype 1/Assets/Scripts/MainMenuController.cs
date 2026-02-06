using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public void OpenSandbox()
    {
        SceneManager.LoadScene(1);
    }

    public void OpenTestLevel()
    {
        SceneManager.LoadScene(2);
    }
}
