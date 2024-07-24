using System.Collections.Generic;
using UnityEngine;

public class EnvironmentSpawner : MonoBehaviour
{
    [SerializeField]
    public GameObject[] objectsToSpawn; // Array of objects to spawn
    [SerializeField]
    public float spawnRadius = 35f; // Radius within which objects will be spawned around the player
    [SerializeField]
    public float despawnDistance = 10f; // Distance at which objects will be despawned on both X and Z axes
    [SerializeField]
    public int poolSize = 250; // Maximum number of objects to pool
    [SerializeField]
    public int maxSpawnPerFrame = 15; // Maximum number of objects to spawn per frame
    [SerializeField]
    public float minDistanceBetweenObjects = 2f; // Minimum distance between spawned objects

    public Transform playerTransform;
    public Camera mainCamera;
    public HashSet<GameObject> activeObjects = new HashSet<GameObject>();
    public Queue<GameObject> objectPool = new Queue<GameObject>();

    void Start()
    {
        if (objectsToSpawn.Length == 0)
        {
            Debug.LogError("No objects assigned to spawn. Please assign objects in the Inspector.");
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player object not found. Please ensure there is a Player GameObject with the tag 'Player' in the scene.");
            return;
        }

        playerTransform = player.transform;

        InitializeObjectPool();
        SpawnObjectsAroundPlayer();
    }

    void Update()
    {
        if (playerTransform == null) return;

        DespawnObjects();
        SpawnObjectsAroundPlayer();
    }

    private void InitializeObjectPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(objectsToSpawn[Random.Range(0, objectsToSpawn.Length)]);
            obj.SetActive(false);
            objectPool.Enqueue(obj);
        }
    }

    private void SpawnObjectsAroundPlayer()
    {
        int spawnedCount = 0;
        while (objectPool.Count > 0 && spawnedCount < maxSpawnPerFrame)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition();
            if (IsPositionValid(spawnPosition))
            {
                GameObject obj = objectPool.Dequeue();
                obj.transform.position = spawnPosition;
                RandomizeObjectProperties(obj);
                obj.SetActive(true);
                activeObjects.Add(obj);
                spawnedCount++;
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * spawnRadius;
        randomDirection.y = -0.2f; // Ensure the objects are placed on the ground
        Vector3 spawnPosition = playerTransform.position + randomDirection;

        // Adjust spawn position to be ahead of the player based on camera direction
        Vector3 cameraForward = mainCamera.transform.forward;
        cameraForward.y = 0; // Ignore vertical direction
        cameraForward.Normalize();

        spawnPosition += cameraForward * spawnRadius * 0.5f;
        return spawnPosition;
    }

    private bool IsPositionValid(Vector3 position)
    {
        foreach (var obj in activeObjects)
        {
            if (Vector3.Distance(obj.transform.position, position) < minDistanceBetweenObjects)
            {
                return false;
            }
        }
        return true;
    }

    private void RandomizeObjectProperties(GameObject obj)
    {
        obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        float scale = Random.Range(0.8f, 1.2f);
        obj.transform.localScale = new Vector3(scale, scale, scale);
    }

    private void DespawnObjects()
    {
        List<GameObject> objectsToDespawn = new List<GameObject>();

        foreach (var obj in activeObjects)
        {
            if (Mathf.Abs(playerTransform.position.x - obj.transform.position.x) > despawnDistance ||
                Mathf.Abs(playerTransform.position.z - obj.transform.position.z) > despawnDistance)
            {
                objectsToDespawn.Add(obj);
            }
        }

        foreach (var obj in objectsToDespawn)
        {
            obj.SetActive(false);
            objectPool.Enqueue(obj);
            activeObjects.Remove(obj);
        }
    }
}
