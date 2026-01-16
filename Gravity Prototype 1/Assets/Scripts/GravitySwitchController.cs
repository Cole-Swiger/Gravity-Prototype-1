using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GravitySwitchController : MonoBehaviour
{
    //Gravity
    public enum GravityDirection { Up, Right, Down, Left };
    [SerializeField] private GravityDirection _direction;
    public GravityDirection direction 
    { 
      get { return _direction;}
      set 
      {
        if (_direction != value)
            {
                _direction = value;
                AssignMaterial();
            }
      }
    }
    [SerializeField] private GameObject[] gravityZones;

    //Materials
    [SerializeField] private Material assignedMaterial;
    [SerializeField] private Material materialDown;
    [SerializeField] private Material materialRight;
    [SerializeField] private Material materialLeft;
    [SerializeField] private Material materialUp;

    //Mode
    [SerializeField] private GameObject actionManager;
    private ActionManagerController.GameMode gameMode;

    private void Awake()
    {
        //modeAction = InputSystem.actions.FindAction("Mode Switch");
    }

    private void OnEnable()
    {
        //modeAction.performed += OnModeActionPerformed;
    }
    private void OnDisable()
    {
        //modeAction.performed -= OnModeActionPerformed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameMode = actionManager.GetComponent<ActionManagerController>().mode;
        AssignMaterial();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchGravityForZones()
    {
        Debug.Log("Switch Method called");
        if (gravityZones.Length > 0) 
        {
            foreach (GameObject go in gravityZones) 
            { 
                GravityZoneController gzc = go.GetComponent<GravityZoneController>();
                gzc.direction = (GravityZoneController.GravityDirection) _direction;
            }
        }
    }

    //Update material to indicate gravity direction that will be applied
    private void AssignMaterial()
    {
        Material switchMaterial;
        switch (_direction)
        {
            case GravityDirection.Up:
                switchMaterial = materialUp;
                break;
            case GravityDirection.Right:
                switchMaterial = materialRight;
                break;
            case GravityDirection.Left:
                switchMaterial = materialLeft;
                break;
            case GravityDirection.Down:
                switchMaterial = materialDown;
                break;
            default:
                switchMaterial = assignedMaterial;
                break;
        }

        GetComponent<MeshRenderer>().material = switchMaterial;
    }

    //Does not work. Switch to Event System
    /*
    private void OnModeActionPerformed(InputAction.CallbackContext context)
    {
        string input = context.control.name;

        switch (input)
        {
            //b for both
            case "b":
                gameMode = ActionManagerController.GameMode.Both;
                break;
            //n for no switches, so free
            case "n":
                gameMode = ActionManagerController.GameMode.Free;
                break;
            //m for more switches, so switch
            case "m":
                gameMode = ActionManagerController.GameMode.Switch;
                break;
        }
        Debug.Log("Current Game Mode: " + gameMode);

        if (isActiveAndEnabled && gameMode == ActionManagerController.GameMode.Free)
        {
            gameObject.SetActive(false);
        }
        else if (!isActiveAndEnabled && (gameMode == ActionManagerController.GameMode.Switch || gameMode == ActionManagerController.GameMode.Both))
        {
            gameObject.SetActive(true);
        }
    }*/
    //TODO: Visually press down while player is in contact
    //Add switches to every wall
    //Work with Action Manager for mode change
}
