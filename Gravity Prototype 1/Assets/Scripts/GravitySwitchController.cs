using UnityEngine;

public class GravitySwitchController : MonoBehaviour
{
    //Gravity
    //public enum GravityDirection { Up, Right, Down, Left };
    [SerializeField] private GravityZoneController.GravityDirection _direction;
    public GravityZoneController.GravityDirection direction 
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
    private ActionManagerController amController;
    private ActionManagerController.GameMode gameMode;
    private bool areSwitchesEnabled = true;

    //Press
    //Used to show button pressing in when player makes contact
    [SerializeField] private float pressValue = 0.04f;

    private void Awake()
    {

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Action manager handles mode switching
        amController = actionManager.GetComponent<ActionManagerController>();
        gameMode = amController.mode;
        //Listen for mode update event from action manager
        amController.modeUpdateEvent.AddListener(OnModeUpdate);
        AssignMaterial();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //When button is pressed and switch is enabled, change gravity direction for associated zone
    public void SwitchGravityForZones()
    {
        Debug.Log("Switch Method called");
        if (areSwitchesEnabled && gravityZones.Length > 0) 
        {
            foreach (GameObject go in gravityZones) 
            { 
                GravityZoneController gzc = go.GetComponent<GravityZoneController>();
                gzc.direction = (GravityZoneController.GravityDirection) _direction;
            }
        }
    }

    //Visually press button down relative to self
    public void PressButton()
    {
        transform.position += transform.up * -pressValue;
    }
    //Visually raise button after player exits trigger
    public void RaiseButton()
    {
        transform.position += transform.up * pressValue;
    }

    //Update material for switch to indicate gravity direction that will be applied
    private void AssignMaterial()
    {
        Material switchMaterial;
        if (areSwitchesEnabled)
        {
            switch (_direction)
            {
                case GravityZoneController.GravityDirection.Up:
                    switchMaterial = materialUp;
                    break;
                case GravityZoneController.GravityDirection.Right:
                    switchMaterial = materialRight;
                    break;
                case GravityZoneController.GravityDirection.Left:
                    switchMaterial = materialLeft;
                    break;
                case GravityZoneController.GravityDirection.Down:
                    switchMaterial = materialDown;
                    break;
                default:
                    switchMaterial = assignedMaterial;
                    break;
            }
        }
        else
        {
            switchMaterial = assignedMaterial;
        }
            GetComponent<MeshRenderer>().material = switchMaterial;
    }

    //On Mode Update event from Action Manager
    private void OnModeUpdate()
    {
        gameMode = amController.mode;
        areSwitchesEnabled = gameMode == ActionManagerController.GameMode.Free ? false : true;
        AssignMaterial();
    }
}
