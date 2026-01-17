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
    //Objects this object is touching that are considered ground
    private HashSet<GameObject> groundedObjects;

    //Movement
    [SerializeField] private float groundSpeed = 3.5f;
    [SerializeField] private float maxGroundSpeed = 3.5f;
    //Force applied from input while jumping an falling
    [SerializeField] private float airForce = 5f;
    //Force applied from input during gravity direction change in air
    [SerializeField] private float gravityAirForce = 2f;
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
            //rb.linearDamping = _useGravityMomentum ? 2f : 0;
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

    //Gravity
    private GravityZoneController.GravityDirection _direction;
    public GravityZoneController.GravityDirection direction
    {
        get { return _direction; }
        set
        {
            _direction = value;
            bool verticalDirection = _direction == GravityZoneController.GravityDirection.Up || _direction == GravityZoneController.GravityDirection.Down;
            movementAxis = verticalDirection ? Axis.X : Axis.Y;
            gravityAxis = verticalDirection ? Axis.Y : Axis.X;
        }
    }
    //Set by gravity zone, default to down
    public Vector3 gravityDirectionVector = Vector3.down;
    //Angle when floor becomes a wall
    [SerializeField] private float floorAngleLimit = .707f; //about 45 degrees
    private enum Axis { X, Y };
    private Axis movementAxis;
    private Axis gravityAxis;

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
        //Set input actions
        //moveAction = InputSystem.actions.FindAction("Move");

        //Store ground objects touching player
        groundedObjects = new HashSet<GameObject>();
        //Defaul air momentum when landing is 0.
        landingMomentum = Vector3.zero;
        movementAxis = Axis.X;
        gravityAxis= Axis.Y;
        //maxLandingMomentum = Vector3.zero;
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
            float signedJumpForce = (_direction == GravityZoneController.GravityDirection.Up || _direction == GravityZoneController.GravityDirection.Right) ? -jumpForce : jumpForce;
            //Assign velocity directly to overcome higher gravity
            rb.linearVelocity = SetVectorByAxis(rb.linearVelocity, gravityAxis, signedJumpForce);
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
        //Left and right gravity allows only up and down movement from player input, and uses the y component of landing momentum. Left and right inputs are ignored
        //Velocity is calculated using player input, ground speed, and landing momentum, if present
        //Only add momentum if going opposite direction of player input to avoid speed boost on landing
        //if multiplication is positive, then momentum and input are in same direction. If no input, still apply momentum
        float moveDirection = GetAxisValue(moveAction.action.ReadValue<Vector2>(), movementAxis);
        float momentumDirection = GetAxisValue(landingMomentum, movementAxis);
        float moveDirectionWithMomentum = moveDirection * momentumDirection;
        float inputSpeed = moveDirection * groundSpeed;
        float inputSpeedWithMomentum = inputSpeed + momentumDirection;
        //Input and momentum are in same direction
        if (moveDirectionWithMomentum > 0)
        {
            //Do not add momentum in same direction
            rb.linearVelocity = SetVectorByAxis(rb.linearVelocity, movementAxis, inputSpeed);
        }
        //Input and momentum are in opposite directions
        else
        {
            rb.linearVelocity = SetVectorByAxis(rb.linearVelocity, movementAxis, inputSpeedWithMomentum);

            //Reduce momentum by amount of opposite (input * speed) to avoid rebounding affect when there is no input from the player
            if (momentumDirection != 0)
            {
                //If momentum crosses 0, cancel it out, else set it to new momentum
                bool areSpeedsDifferentSigns = Mathf.Sign(inputSpeedWithMomentum) != Mathf.Sign(momentumDirection);
                landingMomentum = areSpeedsDifferentSigns ? Vector3.zero : SetVectorByAxis(landingMomentum, movementAxis, inputSpeedWithMomentum);
            }
        }
    }

    //Player movement in air dependant on gravity direction. Floatier than ground movement, preservs momentum
    //User input applies force, then minor corrections are done to handle changing gravity directions and max speeds
    private void MoveInAir()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 targetVelocity = currentVelocity;
        //Allow less force from player during gravity change
        float inputForce = _useGravityMomentum ? gravityAirForce : airForce;
        //Depends on current movement axis
        float inputSpeed = GetAxisValue(moveAction.action.ReadValue<Vector2>(), movementAxis) * inputForce;
        float currentAxisVelocity = GetAxisValue(currentVelocity, movementAxis);

        //Clamp movement speed to max air speed
        if (Mathf.Abs(currentAxisVelocity) > maxAirSpeed)
        {
            //Speed and input both positive
            if (currentAxisVelocity >= 0 && inputSpeed > 0)
            {
                targetVelocity = Vector3.ProjectOnPlane(SetVectorByAxis(targetVelocity, movementAxis, maxAirSpeed), gravityDirectionVector);
            }
            //Speed and input both negative
            else if (currentAxisVelocity < 0 && inputSpeed < 0)
            {
                targetVelocity = Vector3.ProjectOnPlane(SetVectorByAxis(targetVelocity, movementAxis, -maxAirSpeed), gravityDirectionVector);
            }
            //Speed and input are opposite directions. Allow full player input
            else
            {
                targetVelocity = Vector3.ProjectOnPlane(SetVectorByAxis(targetVelocity, movementAxis, inputSpeed), gravityDirectionVector);
            }
        }
        //Speed has not reached max speed yet
        else
        {
            targetVelocity = Vector3.ProjectOnPlane(SetVectorByAxis(targetVelocity, movementAxis, inputSpeed), gravityDirectionVector);
        }

        //Add force using difference between current and target velocites
        rb.AddForce(Vector3.ProjectOnPlane(targetVelocity - currentVelocity, gravityDirectionVector));
        //Correct momentum
        CorrectMomentum();
    }

    //Fix minor momentum issues after air force is applied
    private void CorrectMomentum()
    {
        Vector3 currentVelocity = rb.linearVelocity;
        float moveAxisSpeed = GetAxisValue(currentVelocity, movementAxis);
        float gravityAxisSpeed = GetAxisValue(currentVelocity, gravityAxis);

        //Jumps and regular falling should be set to max speed directly instead of lerping
        if (Mathf.Abs(moveAxisSpeed) > maxAirSpeed && (isJumping || !_useGravityMomentum))
        {
            float finalAirSpeed = moveAxisSpeed < 0 ? -maxAirSpeed : maxAirSpeed;
            rb.linearVelocity = SetVectorByAxis(currentVelocity, movementAxis, finalAirSpeed);
        }

        float correctedMoveSpeed = moveAxisSpeed;
        float correctedGravitySpeed = gravityAxisSpeed;
        //if x or y velocities are still higher than the max speed during gravity change, Lerp them to proper speed
        //This does not apply to speed in direction of gravity
        if (_useGravityMomentum)
        {
            //movement axis speed correction
            if (Mathf.Abs(moveAxisSpeed) > maxAirSpeed)
            {
                correctedMoveSpeed = moveAxisSpeed < 0 ? -maxAirSpeed : maxAirSpeed;
            }
            //gravity axis speed correction
            else if (gravityAxisSpeed < -maxAirSpeed && (_direction == GravityZoneController.GravityDirection.Up || _direction == GravityZoneController.GravityDirection.Right))
            {
                correctedGravitySpeed = -maxAirSpeed;
            }
            else if (gravityAxisSpeed > maxAirSpeed && (_direction == GravityZoneController.GravityDirection.Down || _direction == GravityZoneController.GravityDirection.Left))
            {
                correctedGravitySpeed = maxAirSpeed;
            }
        }
        
        Vector3 correctedVelo = currentVelocity;
        //Correct both axes
        correctedVelo = SetVectorByAxis(correctedVelo, movementAxis, correctedMoveSpeed);
        correctedVelo = SetVectorByAxis(correctedVelo, gravityAxis, correctedGravitySpeed);
        rb.linearVelocity = Vector3.Lerp(currentVelocity, correctedVelo, correction * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter");
        if (other.gameObject.CompareTag("Switch"))
        {
            Debug.Log("Collided with Switch");
            GravitySwitchController gsc = other.gameObject.GetComponent<GravitySwitchController>();
            gsc.PressButton();
            gsc.SwitchGravityForZones();
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Switch"))
        {
            GravitySwitchController gsc = other.gameObject.GetComponent<GravitySwitchController>();
            gsc.RaiseButton();
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
        //When player exits ground collision, remove it from ground set if it was a grounded object
        if (LayerMask.LayerToName(collision.collider.gameObject.layer).Equals("Structure"))
        {
            if (groundedObjects.Contains(collision.collider.gameObject))
            {
                groundedObjects.Remove(collision.collider.gameObject);
            }
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

    //Return the x or y value of a vector, depending on which axis is being used
    private float GetAxisValue(Vector3 v, Axis axis)
    {
        return axis == Axis.X ? v.x : v.y;
    }
    private float GetAxisValue(Vector2 v, Axis axis)
    {
        return axis == Axis.X ? v.x : v.y;
    }

    //Sets the x and y values of a vector depending on the current axis
    //For air movement, inputSpeed and currentSpeed are used for x and y depending on axis
    private Vector3 SetVectorByAxis(Vector3 v, Axis axis, float value)
    {
        if (axis == Axis.X)
        {
            v.x = value;
        }
        else
        {
            v.y = value;
        }
        return v;
    }
 }
