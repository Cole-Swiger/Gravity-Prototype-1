using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    //Physics - Note: Timestep set to 0.01 from 0.02 in Project settings to reduce clipping
    private Rigidbody rb;

    //Grounded checks
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private bool wasGrounded = false;
    [SerializeField] private bool justLanded = false;
    [SerializeField] private float landingDamping = 25f;
    //Objects this object is touching that is considered ground
    private HashSet<GameObject> groundedObjects;

    //Movement
    [SerializeField] private float groundSpeed = 3.5f;
    [SerializeField] private float airSpeed = 5f;
    private Vector3 landingMomentum;
    //[SerializeField] private float snapStrength = 5f;
    public InputActionReference moveAction;
    InputAction jumpAction;
    
    //gravity
    //InputAction gravityAction;
    //InputAction directionAction;
    public enum gravityDirection { Up, Right, Down, Left };
    public gravityDirection direction;
    //Set by gravity zone, default to down
    public Vector3 gravityDirectionVector = Vector3.down;
    /*private gravityDirection previousDir;
    [SerializeField] private Vector3 upGravity = new Vector3(0, -1f, 0);
    [SerializeField] private Vector3 rightGravity = new Vector3(1f, 0, 0);
    [SerializeField] private Vector3 downGravity = new Vector3(0, 1f, 0);
    [SerializeField] private Vector3 leftGravity = new Vector3(-1f, 0, 0);*/
    //Angle when floor becomes a wall
    [SerializeField] private float floorAngleLimit = .707f; //about 45 degrees

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
        groundedObjects = new HashSet<GameObject>();
        landingMomentum = Vector3.zero;
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
        //Check if grounded and calculate landing momentum
        CalculateGroundedState();

        //Movement calculated using player input, momentum, and grounded state
        if (isGrounded)
        {
            MoveOnGround();
        }
        else
        {
            MoveInAir();
        }   
    }

    //Ground movement is tight and precise
    //Momentum from air is present, but diminishes quickly
    private void MoveOnGround()
    {
        if (direction == gravityDirection.Up || direction == gravityDirection.Down)
        {
            rb.linearVelocity = new Vector3((moveAction.action.ReadValue<Vector2>().x * groundSpeed) + landingMomentum.x, 0, 0);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, (moveAction.action.ReadValue<Vector2>().y * groundSpeed) + landingMomentum.y, 0);
        }
            
    }

    //Player movement in air dependant on gravity direction. Floatier than ground movement.
    private void MoveInAir()
    {
        Vector3 currentVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, gravityDirectionVector);
        Vector3 targetVelocity = new Vector3();

        if (direction == gravityDirection.Up || direction == gravityDirection.Down)
        {
            //Ignore up and down inputs from the player, but still keep momentum that slowly fades with drag.
            targetVelocity = new Vector3(moveAction.action.ReadValue<Vector2>().x * airSpeed, rb.linearVelocity.y, 0);
        }
        else
        {
            //Ignore left and right inputs from the player, but still keep momentum that slowly fades with drag.
            targetVelocity = new Vector3(rb.linearVelocity.x, moveAction.action.ReadValue<Vector2>().y * airSpeed, 0);
        }

        rb.AddForce(Vector3.ProjectOnPlane(targetVelocity - currentVelocity, gravityDirectionVector));
    }

    //Check if any objects player is touvhing is ground
    private void OnCollisionStay(Collision collision)
    {
        //Check if player is grounded
        Vector3 up = -gravityDirectionVector;

        foreach (ContactPoint contact in collision.contacts) {
            //Ignore player
            if (LayerMask.LayerToName(collision.collider.gameObject.layer).Equals("Structure"))
            {
                //Calculate direction contacted object is facing.
                //Facing up means the structure is acting as a floor
                float alignment = Vector3.Dot(contact.normal, up);

                if (alignment >= floorAngleLimit)
                {
                    //Duplicates ignored in set. Can exit loop if any are true
                    groundedObjects.Add(collision.collider.gameObject);
                    break;
                }
                else if (groundedObjects.Contains(collision.collider.gameObject)) { 
                    groundedObjects.Remove(collision.collider.gameObject);
                }
            }
        }
    }

    private void CalculateGroundedState()
    {
        //check if object touching ground, and if it just landed
        wasGrounded = isGrounded;
        isGrounded = groundedObjects.Count > 0;
        justLanded = !wasGrounded && isGrounded; //true when going from air to ground
        if (justLanded)
        {
            landingMomentum = rb.linearVelocity;
        }

        //Snap to 0 when close enough
        if (landingMomentum.sqrMagnitude > 0.0001f)
        {
            landingMomentum = Vector3.Lerp(landingMomentum, Vector3.zero, landingDamping * Time.fixedDeltaTime);
        }
        else
        {
            landingMomentum = Vector3.zero;
        }
    }
}
