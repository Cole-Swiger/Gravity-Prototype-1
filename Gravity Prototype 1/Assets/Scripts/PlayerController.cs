using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Movement
    [SerializeField] private float speed = 5f;
    InputAction moveAction;
    InputAction jumpAction;

    //gravity
    InputAction gravityAction;
    //InputAction directionAction;
    private enum gravityDirection { Up, Right, Down, Left };
    [SerializeField] private gravityDirection direction;
    private gravityDirection previousDir;
    [SerializeField] private Vector3 upGravity = new Vector3(0, -1f, 0);
    [SerializeField] private Vector3 rightGravity = new Vector3(1f, 0, 0);
    [SerializeField] private Vector3 downGravity = new Vector3(0, 1f, 0);
    [SerializeField] private Vector3 leftGravity = new Vector3(-1f, 0, 0);

    //mode
    private enum gameMode {Free, Switch, Both};
    [SerializeField] private gameMode mode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set default game mode and gravity
        mode = gameMode.Free;
        direction = gravityDirection.Down;
        previousDir = gravityDirection.Down;

        //Set input actions
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        gravityAction = InputSystem.actions.FindAction("Gravity Switch");
        //directionAction = InputSystem.actions.FindAction("Gravity Direction");
    }

    // Update is called once per frame
    void Update()
    {
        //Move
        if (!gravityAction.IsPressed()) //Not good enough
        {
            Vector2 movement = moveAction.ReadValue<Vector2>();
            transform.Translate(new Vector3(movement.x, 0f, 0f) * speed * Time.deltaTime);
        }

        //Switch gravity in free mode
        if (mode == gameMode.Free || mode == gameMode.Both)
        {
            if (gravityAction.WasPerformedThisFrame())
            {
                string dir = gravityAction.activeControl.name;

                switch (dir) {
                    case "w": case "upArrow":
                        direction = gravityDirection.Up;
                        break;
                    case "d": case "rightArrow":
                        direction = gravityDirection.Right;
                        break;
                    case "s": case "downArrow":
                        direction = gravityDirection.Down;
                        break;
                    case "a": case "leftArrow":
                        direction = gravityDirection.Left;
                        break;
                }
                Debug.Log("Gravity Direction: " + direction);
            }
        }

        //Check Gravity Direction
        if (direction != previousDir)
        {
            switch (direction) {
                case gravityDirection.Up:
                    Physics.gravity = upGravity;
                    break;
                case gravityDirection.Right:
                    Physics.gravity = rightGravity;
                    break;
                case gravityDirection.Down:
                    Physics.gravity = downGravity; 
                    break;
                case gravityDirection.Left:
                    Physics.gravity = leftGravity;
                    break;
            }
            previousDir = direction;
        }
    }
}
