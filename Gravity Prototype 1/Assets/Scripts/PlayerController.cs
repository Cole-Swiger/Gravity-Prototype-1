using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;

    //Movement
    [SerializeField] private float speed = 5f;
    //[SerializeField] private float snapStrength = 5f;
    public InputActionReference moveAction;
    InputAction jumpAction;

    
    //gravity
    //InputAction gravityAction;
    //InputAction directionAction;
    public enum gravityDirection { Up, Right, Down, Left };
    public gravityDirection direction;
    private gravityDirection previousDir;
    [SerializeField] private Vector3 upGravity = new Vector3(0, -1f, 0);
    [SerializeField] private Vector3 rightGravity = new Vector3(1f, 0, 0);
    [SerializeField] private Vector3 downGravity = new Vector3(0, 1f, 0);
    [SerializeField] private Vector3 leftGravity = new Vector3(-1f, 0, 0);

    //mode
    //private enum gameMode {Free, Switch, Both};
    //[SerializeField] private gameMode mode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set default game mode and gravity
        //mode = gameMode.Free;
        //direction = gravityDirection.Down;
        //previousDir = gravityDirection.Down;

        //Set input actions
        //moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
        //gravityAction = InputSystem.actions.FindAction("Gravity Switch");
        //directionAction = InputSystem.actions.FindAction("Gravity Direction");
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Might need reworked
    private void FixedUpdate()
    {
        //Move
        //if (!gravityAction.IsPressed()) //Not good enough
        //{
        //Vector2 movement = moveAction.ReadValue<Vector2>();
        //transform.Translate(new Vector3(movement.x, 0f, 0f) * speed * Time.deltaTime);

        //Vector3 m_Input = new Vector3(moveAction.action.ReadValue<Vector2>().x, 0, 0);
        //rb.MovePosition(transform.position + m_Input * Time.fixedDeltaTime * speed);

        //rb.AddForce(m_Input * speed, ForceMode.VelocityChange);

        Vector3 currentVelocity = new Vector3(); 
        Vector3 targetVelocity = new Vector3();
        Vector3 gravDir = Vector3.down;

        if (direction == gravityDirection.Up)
        {
            currentVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
            //rb.linearVelocity = new Vector3(moveAction.action.ReadValue<Vector2>().x * speed, rb.linearVelocity.y, 0);
            targetVelocity = new Vector3(moveAction.action.ReadValue<Vector2>().x * speed, rb.linearVelocity.y, 0);
            gravDir = Vector3.up;
            //rb.AddForce((new Vector3(moveAction.action.ReadValue<Vector2>().x * speed, 0, 0) - currentVelocity) * snapStrength, ForceMode.VelocityChange);
        }
        else if (direction == gravityDirection.Down)
        {
            currentVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.down);
            targetVelocity = new Vector3(moveAction.action.ReadValue<Vector2>().x * speed, rb.linearVelocity.y, 0);
            gravDir = Vector3.down;
        }
        else if (direction == gravityDirection.Right)
        {
            currentVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.right);
            targetVelocity = new Vector3(rb.linearVelocity.x, moveAction.action.ReadValue<Vector2>().y * speed, 0);
            gravDir = Vector3.right;
        }
        else
        {
            currentVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.left);
            targetVelocity = new Vector3(rb.linearVelocity.x, moveAction.action.ReadValue<Vector2>().y * speed, 0);
            gravDir = Vector3.left;
            //rb.linearVelocity = new Vector3(rb.linearVelocity.x, moveAction.action.ReadValue<Vector2>().y * speed,  0);
            //rb.AddForce((new Vector3(0, moveAction.action.ReadValue<Vector2>().y * speed, 0) - currentVelocity) * snapStrength, ForceMode.VelocityChange);
        }

        rb.AddForce(Vector3.ProjectOnPlane(targetVelocity - currentVelocity, gravDir));
    }
}
