using UnityEngine;
using UnityEngine.InputSystem;

public class GravityZoneController : MonoBehaviour
{
    //gravity
    InputAction gravityAction;
    [SerializeField] private float gravityForce = -29.43f;
    //InputAction directionAction;
    private enum gravityDirection { Up, Right, Down, Left };
    [SerializeField] private gravityDirection direction;
    private Vector3 forceDirection;
    //private gravityDirection previousDir;
    [SerializeField] private Vector3 upGravity;
    [SerializeField] private Vector3 rightGravity;
    [SerializeField] private Vector3 downGravity;
    [SerializeField] private Vector3 leftGravity;

    //mode
    private enum gameMode { Free, Switch, Both };
    [SerializeField] private gameMode mode;

    private void Awake()
    {
        //Called before OnEnable
        gravityAction = InputSystem.actions.FindAction("Gravity Switch");
    }

    private void OnEnable()
    {
        gravityAction.performed += OnGravityActionPerformed;
    }

    private void OnDisable()
    {
        gravityAction.performed -= OnGravityActionPerformed;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = gravityDirection.Down;
        //previousDir = gravityDirection.Down;
        forceDirection = transform.up;

        upGravity = new Vector3(0, gravityForce * -1, 0);
        rightGravity = new Vector3(gravityForce * -1, 0, 0);
        downGravity = new Vector3(0, gravityForce, 0);
        leftGravity = new Vector3(gravityForce, 0, 0);

        mode = gameMode.Free;
    }

    // Update is called once per frame
    void Update()
    {
     
    }

    private void OnTriggerStay(Collider other)
    {
        //Only affect movable objects
        if (other.attachedRigidbody && other.tag == "Movable")
        {   
            switch (direction)
            {
                case gravityDirection.Up:
                    forceDirection = upGravity;
                    break;
                case gravityDirection.Right:
                    forceDirection = rightGravity;
                    break;
                case gravityDirection.Down:
                    forceDirection = downGravity;
                    break;
                case gravityDirection.Left:
                    forceDirection = leftGravity;
                    break;
            }

            //Set direction on player to adjust controls for gravity direction.
            if (other.name == "Player")
            {
                PlayerController pc = other.GetComponent<PlayerController>();
                //Change gravity direction and cancel jumping if gravity direction is different
                if (pc.direction != (PlayerController.gravityDirection) direction)
                {
                    pc.direction = (PlayerController.gravityDirection)direction;
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
        if (mode == gameMode.Free || mode == gameMode.Both)
        {
            /*Debug.Log("Action performed: " + context);
            Debug.Log("Active Control: " + context.control);
            Debug.Log("Active Control Name: " + context.control.name);*/
            string dir = context.control.name;

            switch (dir)
            {
                case "w":
                case "upArrow":
                    direction = gravityDirection.Up;
                    break;
                case "d":
                case "rightArrow":
                    direction = gravityDirection.Right;
                    break;
                case "s":
                case "downArrow":
                    direction = gravityDirection.Down;
                    break;
                case "a":
                case "leftArrow":
                    direction = gravityDirection.Left;
                    break;
            }
        }
    }
}
