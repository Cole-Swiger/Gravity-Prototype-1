using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using TMPro;

public class ActionManagerController : MonoBehaviour
{
    //Mode
    private InputAction modeAction;
    private InputAction cameraModeAction;
    private InputAction pauseAction;
    public enum GameMode { Free, Switch, Both };
    public GameMode mode;

    //Events
    //Used to tell other classes when the game mode is updated
    public UnityEvent modeUpdateEvent;
    public UnityEvent cameraModeUpdateEvent;

    //Menus and Text
    [SerializeField] private TMP_Text modeText;
    [SerializeField] private GameObject pauseMenu;

    //State
    public bool isPaused = false;

    private void Awake()
    {
        modeAction = InputSystem.actions.FindAction("Mode Switch");
        cameraModeAction = InputSystem.actions.FindAction("Camera Mode Switch");
        pauseAction = InputSystem.actions.FindAction("Pause");
    }

    private void OnEnable()
    {
        modeAction.performed += OnModeActionPerformed;
        cameraModeAction.performed += OnCameraModeActionPerformed;
        pauseAction.performed += OnPauseActionPerformed;
    }
    private void OnDisable()
    {
        modeAction.performed -= OnModeActionPerformed;
        cameraModeAction.performed -= OnCameraModeActionPerformed;
        pauseAction.performed -= OnPauseActionPerformed;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Update Game Mode when action is performed
    //Game mode affects if switches work or if actions can update gravity
    private void OnModeActionPerformed(InputAction.CallbackContext context)
    {
        string input = context.control.name;

        switch (input)
        {
            //b for both
            case "b":
                mode = GameMode.Both;
                if (modeText != null)
                {
                    modeText.text = "Both";
                }
                break;
            //n for no switches, so free
            case "n":
                mode = GameMode.Free;
                if (modeText != null)
                {
                    modeText.text = "Manual";
                }
                break;
            //m for more switches, so switch
            case "m":
                mode = GameMode.Switch;
                if (modeText != null)
                {
                    modeText.text = "Switch";
                }
                break;
        }
        Debug.Log("Current Game Mode: " + mode);
        //Invoke event to inform other classes
        modeUpdateEvent.Invoke();
    }

    //Update Camera Mode when action is performed
    private void OnCameraModeActionPerformed(InputAction.CallbackContext context)
    {
        cameraModeUpdateEvent.Invoke();
    }

    //Pause and Unpause Scene
    //Show and hide pause menu
    public void OnPauseActionPerformed(InputAction.CallbackContext context)
    {
        isPaused = !isPaused;
        Time.timeScale = isPaused ? 0 : 1;
        pauseMenu.SetActive(isPaused);
    }
}
