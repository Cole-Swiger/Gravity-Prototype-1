using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;

public class PlayerController : MonoBehaviour
{
    //Physics - Note: Timestep set to 0.01 from 0.02 in Project settings to reduce clipping
    [SerializeField] private Rigidbody rb;

    //Grounded checks
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private bool wasGrounded = false;
    [SerializeField] private bool justLanded = false;
    [SerializeField] private float landingDamping = 25f;
    //Objects this object is touching that is considered ground
    private HashSet<GameObject> groundedObjects;

    //Movement
    [SerializeField] private float groundSpeed = 3.5f;
    [SerializeField] private float maxGroundSpeed = 3.5f;
    [SerializeField] private float airForce = 5f;
    [SerializeField] private float maxAirSpeed = 5f;
    [SerializeField] private float correction = 20f;
    private Vector3 landingMomentum;
    //private Vector3 maxLandingMomentum;
    //[SerializeField] private float snapStrength = 5f;
    public InputActionReference moveAction;
    //Test movement feel in air with toggle
    [SerializeField] private bool allowAirMovement = true;
    [SerializeField] private AnimationCurve airSpeedSnap;
    private bool _useGravityMomentum = false;
    public bool useGravityMomentum
    {
        get { return _useGravityMomentum; }
        set
        {
            _useGravityMomentum = value;
            rb.linearDamping = _useGravityMomentum ? 2f : 0;
        }
    }

    //Jump
    InputAction jumpAction;
    [SerializeField] private float jumpForce = 5f;
    //Prevent full speed on landing impact from jump/falling
    //Set buffer timer between 0-1. Lower number means lower ground speed on impact
    [SerializeField] private float landingBufferTime = 1f;
    //Set this to buffer timer on landing to reduce speed
    private float landingTimer = 1f;
    public bool isJumping = false;

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

    private void Awake()
    {
        //Assign rigidbody and input actions
        rb = GetComponent<Rigidbody>();
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    //Turn On/Off input actions when object is enabled/disabled
    private void OnEnable()
    {
        jumpAction.performed += OnJumpAction;
    }
    private void OnDisable()
    {
        jumpAction.performed -= OnJumpAction;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Set default game mode and gravity
        //mode = gameMode.Free;
        //direction = gravityDirection.Down;
        //previousDir = gravityDirection.Down;

        //Set input actions
        //moveAction = InputSystem.actions.FindAction("Move");

        //Store ground objects touching player
        groundedObjects = new HashSet<GameObject>();
        //Defaul air momentum when landing is 0.
        landingMomentum = Vector3.zero;
        //maxLandingMomentum = Vector3.zero;
        //gravityAction = InputSystem.actions.FindAction("Gravity Switch");
        //directionAction = InputSystem.actions.FindAction("Gravity Direction");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        //Check if grounded and calculate landing momentum
        CalculateGroundedState();
        //If landing momentum present, naturally reduce it over time. Player input also lowers momentum separately
        CalculateNaturalMomentum();

        //Movement calculated using player input, momentum, and grounded state
        if (isGrounded)
        {
            MoveOnGround();
        }
        //Only allow air input while toggle is true
        else if (allowAirMovement)
        {
            MoveInAir();
        }   
    }

    //Action to take when Jump button is pressed.
    private void OnJumpAction(InputAction.CallbackContext context)
    {
        Debug.Log("Jump action performed");
        //Only jump if on ground or on grounded object
        if (isGrounded)
        {
            //Direction determines which way to add velocity
            //Assign velocity directly to overcome higher gravity
            switch (direction)
            {
                case gravityDirection.Up:
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, -jumpForce, 0f);
                    break;
                case gravityDirection.Right:
                    rb.linearVelocity = new Vector3(-jumpForce, rb.linearVelocity.y, 0f);
                    break;
                case gravityDirection.Down:
                    rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, 0f);
                    break;
                case gravityDirection.Left:
                    rb.linearVelocity = new Vector3(jumpForce, rb.linearVelocity.y, 0f);
                    break;
            }
            isJumping = true;
        }
    }

    //Ground movement is tight and precise
    //Momentum from air is present, but diminishes quickly over time and by player input
    private void MoveOnGround()
    {
        //Briefly reduce ground speed if landing timer is being used to simulate ground impact
        //Set landing timer and max landing timer to 1 to ignore effect entirely
        if (landingTimer < 1f)
        {
            groundSpeed = maxGroundSpeed * landingTimer;
            landingTimer += Time.deltaTime;
        }
        else
        {
            //Use full speed
            groundSpeed = maxGroundSpeed;
            landingTimer = 1f;
        }

        //Up and down gravitry allows only left and right movement from player input, and uses the x component of landing momentum. Up and down inputs are ignored
        if (direction == gravityDirection.Up || direction == gravityDirection.Down)
        {
            //Velocity is calculated using player input, ground speed, and landing momentum, if present
            //Only add momentum if going opposite direction of player input to avoid speed boost on landing
            //if multiplication is positive, then momentum and input are in same direction. If no input, still apply momentum
            float moveDirection = moveAction.action.ReadValue<Vector2>().x * landingMomentum.x;
            //Input and momentum are in same direction
            if (moveDirection > 0)
            {
                //Do not add momentum in same direction
                rb.linearVelocity = new Vector3(moveAction.action.ReadValue<Vector2>().x * groundSpeed, rb.linearVelocity.y, 0);
            }
            //Input and momentum are in opposite directions
            else
            {
                rb.linearVelocity = new Vector3((moveAction.action.ReadValue<Vector2>().x * groundSpeed) + landingMomentum.x, rb.linearVelocity.y, 0);

                //Reduce momentum by amount of opposite (input * speed) to avoid rebounding affect when there is no input from the player
                if (landingMomentum.x != 0)
                {
                    float newMomentum = landingMomentum.x + (moveAction.action.ReadValue<Vector2>().x * groundSpeed);
                    //If momentum crosses 0, cancel it out, else set it to new momentum
                    landingMomentum = (Mathf.Sign(newMomentum) != Mathf.Sign(landingMomentum.x)) ? Vector3.zero : new Vector3(newMomentum, 0, 0);
                }
            }
        }
        //Left and right gravity allows only up and down movement from player input, and uses the y component of landing momentum. Left and right inputs are ignored
        else
        {
            //Velocity is calculated using player input, ground speed, and landing momentum, if present
            //Only add momentum if going opposite direction of player input to avoid speed boost on landing
            //if multiplication is positive, then momentum and input are in same direction. If no input, still apply momentum
            float moveDirection = moveAction.action.ReadValue<Vector2>().y * landingMomentum.y;
            //Input and momentum are in same direction
            if (moveDirection > 0)
            {
                //Do not add momentum in same direction
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, moveAction.action.ReadValue<Vector2>().y * groundSpeed, 0);
            }
            //Input and momentum are in opposite directions
            else
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, (moveAction.action.ReadValue<Vector2>().y * groundSpeed) + landingMomentum.y, 0);

                //Reduce momentum by amount of opposite (input * speed) to avoid rebounding affect when there is no input from the player
                if (landingMomentum.y != 0)
                {
                    float newMomentum = landingMomentum.y + (moveAction.action.ReadValue<Vector2>().y * groundSpeed);
                    //If momentum crosses 0, cancel it out, else set it to new momentum
                    landingMomentum = (Mathf.Sign(newMomentum) != Mathf.Sign(landingMomentum.y)) ? Vector3.zero : new Vector3(0, newMomentum, 0);
                }
            }
        }         
    }

    //
    //TODO: Refactor to reduce duplicate code
    //

    //Player movement in air dependant on gravity direction. Floatier than ground movement, preservs momentum
    //User input applies force, then minor corrections are done to handle changing gravity directions and max speeds
    private void MoveInAir()
    {
        Vector3 currentVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, gravityDirectionVector);
        Vector3 targetVelocity = new Vector3();

        //Up and Down gravity calculations
        if (direction == gravityDirection.Up || direction == gravityDirection.Down)
        {
            //Ignore up and down inputs from the player, but still keep momentum that slowly fades with drag.
            float inputSpeed = moveAction.action.ReadValue<Vector2>().x * airForce;

            //If magnitude of current x velocity is greater than max air speed, set the target speed to the max air speed for the x axis
            //Use user input if it is opposite direction of current velocity
            if (Mathf.Abs(rb.linearVelocity.x) > maxAirSpeed)
            {
                if (rb.linearVelocity.x >= 0 && inputSpeed > 0)
                {
                    targetVelocity = Vector3.ProjectOnPlane(new Vector3(maxAirSpeed, rb.linearVelocity.y, 0), gravityDirectionVector);
                }
                else if (rb.linearVelocity.x < 0 && inputSpeed < 0)
                {
                    targetVelocity = Vector3.ProjectOnPlane(new Vector3(-maxAirSpeed, rb.linearVelocity.y, 0), gravityDirectionVector);
                }
                //Speed and input are opposite directions
                else 
                {
                    targetVelocity = Vector3.ProjectOnPlane(new Vector3(inputSpeed, rb.linearVelocity.y, 0), gravityDirectionVector);
                }
            }
            //max speed not met yet
            else
            {
                targetVelocity = Vector3.ProjectOnPlane(new Vector3(inputSpeed, rb.linearVelocity.y, 0), gravityDirectionVector);
            }
            //Add force using difference between current and target velocites
            rb.AddForce(Vector3.ProjectOnPlane(targetVelocity - currentVelocity, gravityDirectionVector));
            
            //Correct minor momentum issues and enforce horizontal max air speed while jumping or falling without gravity change
            float currentX = rb.linearVelocity.x;
            float currentY = rb.linearVelocity.y;
            //Jumps and regular falling should not be affected by input force
            if (Mathf.Abs(currentX) > maxAirSpeed && (isJumping || !_useGravityMomentum))
            {
                //Set velocity directly
                rb.linearVelocity = currentX < 0 ? new Vector3(-maxAirSpeed, currentY, 0) : new Vector3(maxAirSpeed, currentY, 0);
            }

            float correctedX = currentX;
            float correctedY = currentY;
            //if x or y velocities are still higher than the max speed, Lerp them to proper speed
            //This does not apply to speed in direction of gravity
            if (_useGravityMomentum && Mathf.Abs(currentX) > maxAirSpeed)
            {
                correctedX = currentX < 0 ? -maxAirSpeed : maxAirSpeed;
            }
            else if (_useGravityMomentum && currentY < -maxAirSpeed && direction == gravityDirection.Up)
            {
                correctedY = -maxAirSpeed;
            }
            else if (_useGravityMomentum && currentY > maxAirSpeed && direction == gravityDirection.Down)
            {
                correctedY = maxAirSpeed;
            }
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(correctedX, correctedY, 0), correction * Time.fixedDeltaTime);
        }
        //Left and Right gravity calculations
        else
        {
            //Ignore left and right inputs from the player, but still keep momentum that slowly fades with drag.
            float inputSpeed = moveAction.action.ReadValue<Vector2>().y * airForce;

            //If magnitude of current y velocity is greater than max air speed, set the target speed to the max air speed for the y axis
            //Use user input if it is opposite direction of current velocity
            if (Mathf.Abs(rb.linearVelocity.y) > maxAirSpeed)
            {
                if (rb.linearVelocity.y >= 0 && inputSpeed > 0)
                {
                    targetVelocity = Vector3.ProjectOnPlane(new Vector3(rb.linearVelocity.x, maxAirSpeed, 0), gravityDirectionVector);
                }
                else if (rb.linearVelocity.y < 0 && inputSpeed < 0)
                {
                    targetVelocity = Vector3.ProjectOnPlane(new Vector3(rb.linearVelocity.x, -maxAirSpeed, 0), gravityDirectionVector);
                }
                else
                {
                    targetVelocity = Vector3.ProjectOnPlane(new Vector3(rb.linearVelocity.x, inputSpeed, 0), gravityDirectionVector);
                }
            }
            //Speed and input are opposite directions
            else
            {
                targetVelocity = Vector3.ProjectOnPlane(new Vector3(rb.linearVelocity.x, inputSpeed, 0), gravityDirectionVector);
            }
            //Add force using difference between current and target velocites
            rb.AddForce(Vector3.ProjectOnPlane(targetVelocity - currentVelocity, gravityDirectionVector));

            //Correct minor momentum issues and enforce horizontal max air speed while jumping or falling without gravity change
            float currentX = rb.linearVelocity.x;
            float currentY = rb.linearVelocity.y;
            //Jumps and regular falling should not be affected by input force
            if (Mathf.Abs(rb.linearVelocity.y) > maxAirSpeed && (isJumping || !_useGravityMomentum))
            {
                rb.linearVelocity = currentY < 0 ? new Vector3(currentX, -maxAirSpeed, 0) : new Vector3(currentX, maxAirSpeed, 0);
            }

            float correctedX = currentX;
            float correctedY = currentY;
            //if x or y velocities are still higher than the max speed, Lerp them to proper speed
            //This does not apply to speed in direction of gravity
            if (_useGravityMomentum && Mathf.Abs(currentY) > maxAirSpeed)
            {
                correctedY = currentY < 0 ? -maxAirSpeed : maxAirSpeed;
            }
            else if (_useGravityMomentum && currentX < -maxAirSpeed && direction == gravityDirection.Right)
            {
                correctedX = -maxAirSpeed;
            }
            else if (_useGravityMomentum && currentX > maxAirSpeed && direction == gravityDirection.Left)
            {
                correctedX = maxAirSpeed;
            }
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, new Vector3(correctedX, correctedY, 0), correction * Time.fixedDeltaTime);
        }
    }

    //Check if any objects player is touching is ground
    private void OnCollisionStay(Collision collision)
    {
        //Up is opposite of gravity direction
        Vector3 up = -gravityDirectionVector;

        //Each collision will have multiple contact points. If any contact points from any collision are considered ground, the player is grounded.
        foreach (ContactPoint contact in collision.contacts) {
            //Ignore player
            if (LayerMask.LayerToName(collision.collider.gameObject.layer).Equals("Structure"))
            {
                //Calculate direction contacted object is facing.
                //Facing up means the structure is acting as a floor
                float alignment = Vector3.Dot(contact.normal, up);

                if (alignment >= floorAngleLimit)
                {
                    //Add ground to set. If any objects are in this set, the player is grounded
                    //Duplicates ignored in set. Can exit loop if any are true
                    groundedObjects.Add(collision.collider.gameObject);
                    break;
                }
                //Object is wall or ceiling
                else if (groundedObjects.Contains(collision.collider.gameObject)) 
                { 
                    //If collided non-grounded object in set, remove it
                    groundedObjects.Remove(collision.collider.gameObject);
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        //When player exits collision, remove it from ground set if it was a grounded object
        if (groundedObjects.Contains(collision.collider.gameObject))
        {
            groundedObjects.Remove(collision.collider.gameObject);
        }
    }

    //Used to determine if the player is currently on the ground or in a grounded state
    private void CalculateGroundedState()
    {
        //check if object touching ground, and if it just landed
        wasGrounded = isGrounded;
        isGrounded = groundedObjects.Count > 0;
        justLanded = !wasGrounded && isGrounded; //true when going from air to ground
        //isJumping = isGrounded ? false : isJumping; //Ensure false while grounded
    }

    //Reduce momentum overtime regardless of player input
    private void CalculateNaturalMomentum()
    {
        //At moment player goes from air to ground
        if (justLanded)
        {
            //At time of landing, set landing momentum to the velocity the player had in the air in relation to gravity
            landingMomentum = Vector3.ProjectOnPlane(rb.linearVelocity, gravityDirectionVector);
            //Used to reduce speed on impact if buffer is less than 1
            landingTimer = landingBufferTime;
            isJumping = false;
            useGravityMomentum = false;
        }

        //Snap momentum to 0 when close enough
        if (landingMomentum.sqrMagnitude > 0.0001f)
        {
            //maxLandingMomentum = Vector3.ProjectOnPlane(rb.linearVelocity, gravityDirectionVector);
            landingMomentum = Vector3.Lerp(landingMomentum, Vector3.zero, landingDamping * Time.fixedDeltaTime);
        }
        else
        {
            landingMomentum = Vector3.zero;
            //maxLandingMomentum = Vector3.zero;
        }
    }
}
