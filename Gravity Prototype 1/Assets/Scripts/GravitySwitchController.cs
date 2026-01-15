using System.Collections;
using UnityEngine;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
                gzc.direction = (GravityZoneController.GravityDirection) direction;
            }
        }
    }

    //Update material to indicate gravity direction that will be applied
    private void AssignMaterial()
    {
        Material switchMaterial;
        switch (direction)
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
    //TODO: Visually press down while player is in contact
    //Add switches to every wall
}
