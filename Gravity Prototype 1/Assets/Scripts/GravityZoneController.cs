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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = gravityDirection.Down;
        //previousDir = gravityDirection.Down;
        forceDirection = transform.up;
        gravityAction = InputSystem.actions.FindAction("Gravity Switch");

        upGravity = new Vector3(0, gravityForce * -1, 0);
        rightGravity = new Vector3(gravityForce * -1, 0, 0);
        downGravity = new Vector3(0, gravityForce, 0);
        leftGravity = new Vector3(gravityForce, 0, 0);

        mode = gameMode.Free;
    }

    // Update is called once per frame
    void Update()
    {
        //Switch gravity in free mode
        if (mode == gameMode.Free || mode == gameMode.Both)
        {
            if (gravityAction.WasPerformedThisFrame())
            {
                string dir = gravityAction.activeControl.name;

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
                Debug.Log("Gravity Direction: " + direction);
            }
        }
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
                pc.direction = (PlayerController.gravityDirection) direction;
                pc.gravityDirectionVector = forceDirection.normalized;
            }

            other.attachedRigidbody.AddForce(forceDirection, ForceMode.Acceleration);
        }
    }

    //TODO: Figure out why there is occasionally a null pointer exception when checking for active control (input) on line 47
}
