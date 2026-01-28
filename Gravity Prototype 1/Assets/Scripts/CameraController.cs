using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Mode
    [SerializeField] private GameObject actionManager;
    private ActionManagerController amController;
    private Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Action manager handles camera mode switching
        amController = actionManager.GetComponent<ActionManagerController>();
        amController.cameraModeUpdateEvent.AddListener(OnCameraModeUpdate);
        cam = GetComponent<Camera>();
    }

    //Update Camera Mode.
    //Triggered by Action Manager Event
    private void OnCameraModeUpdate()
    {
        cam.orthographic = !cam.orthographic;
    }
}
