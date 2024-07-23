using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject levelupUI;
    public  bool isPlayerDead = false;
    public GameObject defaultHud;
    public GameObject deathHud; // The enemy prefab
    public GameObject enemyPrefab; // The enemy prefab
    public GameObject healingPotionPrefab; // The healing potion prefab

    [Header("Player Settings")]
    public Transform playerTransform; // The player's transform

    [Header("Spawn Settings")]
    public float enemySpawnRangeX = 20f; // Range for spawning enemies on the X axis
    public float enemySpawnRangeZ = 20f; // Range for spawning enemies on the Z axis
    public int enemyBaseSpawnCount = 10; // Base number of enemies to spawn

    public float potionSpawnRangeX = 20f; // Range for spawning potions on the X axis
    public float potionSpawnRangeZ = 20f; // Range for spawning potions on the Z axis
    public int potionBaseSpawnCount = 1; // Base number of potions to spawn

    private int currentLvl = 1;

    void Start()
    {
        SpawnEnemies();
        SpawnPotions();
    }

    // Method to spawn enemies in random locations around the player
    private void SpawnEnemies()
    {
        int enemySpawnCount = enemyBaseSpawnCount + (currentLvl - 1) * 10;
        for (int i = 0; i < enemySpawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition(enemySpawnRangeX, enemySpawnRangeZ);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    // Method to spawn healing potions in random locations around the player
    private void SpawnPotions()
    {
        int potionSpawnCount = potionBaseSpawnCount + (currentLvl - 1);
        for (int i = 0; i < potionSpawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition(potionSpawnRangeX, potionSpawnRangeZ);
            Instantiate(healingPotionPrefab, spawnPosition, Quaternion.identity);
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
        if (isPlayerDead)
        {
            deathHud.SetActive(true);
            defaultHud.SetActive(false);
            StartCoroutine(RestartGame());

            // Handle game over logic here
        }
        CheckEnemies();
    }

    void CheckEnemies()
    {
        // Find all game objects with the tag "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Check if the length of the array is zero
        if (enemies.Length == 0)
        {
            levelupUI.SetActive(true);
            StartCoroutine(AllEnemiesKilled());
        }
    }

    IEnumerator AllEnemiesKilled()
    {
        yield return new WaitForSeconds(5);
        levelupUI.SetActive(false);
        currentLvl++;
        SpawnEnemies();
        SpawnPotions();
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }
}
