using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool isPlayerDead = false;
    public GameObject objectToSpawn; // The game object to be spawned
    public Transform playerTransform; // The player's transform
    public float spawnRangeX = 20f; // Range for spawning on the X axis
    public float spawnRangeZ = 20f; // Range for spawning on the Z axis
    public int maxSpawnCount = 10; // Maximum number of times to spawn the object

    private int currentSpawnCount = 0; // Counter to keep track of spawned objects

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnObjects();
    }

    // Method to spawn objects in random locations around the player
    void SpawnObjects()
    {
        for (int i = 0; i < maxSpawnCount; i++)
        {
            Vector3 spawnPosition = new Vector3(
                playerTransform.position.x + Random.Range(-spawnRangeX, spawnRangeX),
                playerTransform.position.y,
                playerTransform.position.z + Random.Range(-spawnRangeZ, spawnRangeZ)
            );

            Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
            currentSpawnCount++;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}