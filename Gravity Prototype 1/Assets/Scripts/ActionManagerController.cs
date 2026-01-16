using UnityEngine;
using UnityEngine.InputSystem;

public class ActionManagerController : MonoBehaviour
{
    //Mode
    private InputAction modeAction;
    public enum GameMode { Free, Switch, Both };
    [SerializeField] public GameMode mode;

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

    //Update Game Mode
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
    }

    //TODO:
    //Make it like an event system where actions happen here then are sent to relevent objects
}
