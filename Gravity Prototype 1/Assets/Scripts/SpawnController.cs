using UnityEngine;

public class SpawnController : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private Vector3 spawnPoint;
    private Quaternion spawnRotation;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnPoint = new Vector3(transform.position.x, transform.position.y, 0);
        spawnRotation = Quaternion.identity;
        if (player != null) 
        {
            player.transform.position = spawnPoint;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
