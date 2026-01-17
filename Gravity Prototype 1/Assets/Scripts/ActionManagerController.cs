using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class ActionManagerController : MonoBehaviour
{
    //Mode
    private InputAction modeAction;
    public enum GameMode { Free, Switch, Both };
    public GameMode mode;

    //Events
    //Used to tell other classes when the game mode is updated
    public UnityEvent modeUpdateEvent;

    private void Awake()
    {
        modeAction = InputSystem.actions.FindAction("Mode Switch");
    }

    private void OnEnable()
    {
        modeAction.performed += OnModeActionPerformed;
    }
    private void OnDisable()
    {
        modeAction.performed -= OnModeActionPerformed;
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
                break;
            //n for no switches, so free
            case "n":
                mode = GameMode.Free;
                break;
            //m for more switches, so switch
            case "m":
                mode = GameMode.Switch;
                break;
        }
        Debug.Log("Current Game Mode: " + mode);
        //Invoke event to inform other classes
        modeUpdateEvent.Invoke();
    }
}
