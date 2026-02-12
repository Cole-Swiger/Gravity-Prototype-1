using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject actionManager;
    //Unpause game
    public void OnContinuePress()
    {
        Time.timeScale = 1;
        actionManager.GetComponent<ActionManagerController>().isPaused = false;
        gameObject.SetActive(false);
    }

    //Quit to main menu
    public void OnQuitPress()
    {
        Time.timeScale = 1;
        actionManager.GetComponent<ActionManagerController>().isPaused = false;
        SceneManager.LoadScene(0);
    }
}
