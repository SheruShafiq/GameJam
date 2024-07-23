using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject defaultHud;
    public GameObject deathHud; // The enemy prefab
    public GameObject enemyPrefab; // The enemy prefab
    public GameObject healingPotionPrefab; // The healing potion prefab

    [Header("Player Settings")]
    public Transform playerTransform; // The player's transform

    [Header("Spawn Settings")]
    public float enemySpawnRangeX = 20f; // Range for spawning enemies on the X axis
    public float enemySpawnRangeZ = 20f; // Range for spawning enemies on the Z axis
    public int enemyMaxSpawnCount = 10; // Maximum number of enemies to spawn

    public float potionSpawnRangeX = 20f; // Range for spawning potions on the X axis
    public float potionSpawnRangeZ = 20f; // Range for spawning potions on the Z axis
    public int potionMaxSpawnCount = 10; // Maximum number of potions to spawn

    private int currentEnemySpawnCount = 0; // Counter for spawned enemies
    private int currentPotionSpawnCount = 0; // Counter for spawned potions

    public bool isPlayerDead = false;

    void Start()
    {
        SpawnEnemies();
        SpawnPotions();
    }

    // Method to spawn enemies in random locations around the player
    private void SpawnEnemies()
    {
        for (int i = 0; i < enemyMaxSpawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition(enemySpawnRangeX, enemySpawnRangeZ);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            currentEnemySpawnCount++;
        }
    }

    // Method to spawn healing potions in random locations around the player
    private void SpawnPotions()
    {
        for (int i = 0; i < potionMaxSpawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition(potionSpawnRangeX, potionSpawnRangeZ);
            Instantiate(healingPotionPrefab, spawnPosition, Quaternion.identity);
            currentPotionSpawnCount++;
        }
    }

    // Method to get a random spawn position around the player
    private Vector3 GetRandomSpawnPosition(float rangeX, float rangeZ)
    {
        return new Vector3(
            playerTransform.position.x + Random.Range(-rangeX, rangeX),
            playerTransform.position.y,
            playerTransform.position.z + Random.Range(-rangeZ, rangeZ)
        );
    }
   

    void Update()
    {
        if(isPlayerDead)
        {
            deathHud.SetActive(true);
            defaultHud.SetActive(false);
            StartCoroutine(RestartGame());
            
         // change scene to scene 0
            // Handle game over logic here
        }
        // Add any update logic here, if needed in the future
    }
     IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }
}
