using UnityEngine;
using UnityEngine.InputSystem;

public class GravityZoneController : MonoBehaviour
{
    //gravity
    private InputAction gravityAction;
    [SerializeField] private float gravityForce = -29.43f;
    public enum GravityDirection { Up, Right, Down, Left };
    private GravityDirection _direction;
    public GravityDirection direction
    {
        get { return _direction; }
        set
        {
            if (_direction != value)
            {
                _direction = value;
                AssignMaterial();
            }
        }
    }
    private Vector3 forceDirection;
    //private GravityDirection previousDir;
    [SerializeField] private Vector3 upGravity;
    [SerializeField] private Vector3 rightGravity;
    [SerializeField] private Vector3 downGravity;
    [SerializeField] private Vector3 leftGravity;

    //mode
    //private InputAction modeAction;
    //private enum GameMode { Free, Switch, Both };
    [SerializeField] private GameObject actionManager;
    private ActionManagerController.GameMode mode;

    //Materials
    [SerializeField] private Material assignedMaterial;
    [SerializeField] private Material materialDown;
    [SerializeField] private Material materialRight;
    [SerializeField] private Material materialLeft;
    [SerializeField] private Material materialUp;

    private void Awake()
    {
        //Called before OnEnable
        gravityAction = InputSystem.actions.FindAction("Gravity Switch");
        //modeAction = InputSystem.actions.FindAction("Mode Switch");
    }

    private void OnEnable()
    {
        gravityAction.performed += OnGravityActionPerformed;
        //modeAction.performed += OnModeActionPerformed;
    }

    private void OnDisable()
    {
        gravityAction.performed -= OnGravityActionPerformed;
        //modeAction.performed -= OnModeActionPerformed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = GravityDirection.Down;
        //previousDir = GravityDirection.Down;
        forceDirection = transform.up;

        upGravity = new Vector3(0, gravityForce * -1, 0);
        rightGravity = new Vector3(gravityForce * -1, 0, 0);
        downGravity = new Vector3(0, gravityForce, 0);
        leftGravity = new Vector3(gravityForce, 0, 0);

        mode = actionManager.GetComponent<ActionManagerController>().mode;
        AssignMaterial();
    }

    // Update is called once per frame
    void Update()
    {
        mode = actionManager.GetComponent<ActionManagerController>().mode;
    }

    private void OnTriggerStay(Collider other)
    {
        //Only affect movable objects
        if (other.attachedRigidbody && other.tag == "Movable")
        {   
            switch (_direction)
            {
                case GravityDirection.Up:
                    forceDirection = upGravity;
                    break;
                case GravityDirection.Right:
                    forceDirection = rightGravity;
                    break;
                case GravityDirection.Down:
                    forceDirection = downGravity;
                    break;
                case GravityDirection.Left:
                    forceDirection = leftGravity;
                    break;
            }

            //Set direction on player to adjust controls for gravity direction.
            if (other.name == "Player")
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                //Change gravity direction and cancel jumping if gravity direction is different
                if (pc.direction != _direction)
                {
                    pc.direction = _direction;
                    pc.gravityDirectionVector = forceDirection.normalized;
                    //Use gravity momentum and physics instead of jumping
                    pc.isJumping = false;
                    pc.useGravityMomentum = true;
                }       
            }

            //Apply gravity force
            other.attachedRigidbody.AddForce(forceDirection, ForceMode.Acceleration);
        }
    }

    private void OnGravityActionPerformed(InputAction.CallbackContext context)
    {
        if (mode == ActionManagerController.GameMode.Free || mode == ActionManagerController.GameMode.Both)
        {
            /*Debug.Log("Action performed: " + context);
            Debug.Log("Active Control: " + context.control);
            Debug.Log("Active Control Name: " + context.control.name);*/
            string dir = context.control.name;

            switch (dir)
            {
                case "w":
                case "upArrow":
                    direction = GravityDirection.Up;
                    break;
                case "d":
                case "rightArrow":
                    direction = GravityDirection.Right;
                    break;
                case "s":
                case "downArrow":
                    direction = GravityDirection.Down;
                    break;
                case "a":
                case "leftArrow":
                    direction = GravityDirection.Left;
                    break;
            }
        }
    }

    //Update material to indicate gravity direction is applied
    //Change back wall color to match gravity direction
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

        if (transform.parent != null) 
        {
            transform.parent.Find("Back Wall").GetComponent<MeshRenderer>().material = switchMaterial;
        }     
    }
}
