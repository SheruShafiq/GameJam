using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject levelupUI;
    public bool isPlayerDead = false;
    public GameObject defaultHud;
    public GameObject deathHud;
    public GameObject enemyPrefab;
    public GameObject healingPotionPrefab;

    [Header("Player Settings")]
    public Transform playerTransform;

    [Header("Spawn Settings")]
    public float enemySpawnRangeX = 20f;
    public float enemySpawnRangeZ = 20f;
    public int enemyBaseSpawnCount = 10;

    public float potionSpawnRangeX = 20f;
    public float potionSpawnRangeZ = 20f;
    public int potionBaseSpawnCount = 1;

    private int currentLvl = 1;
    private bool isSpawningEnemies = false; // Flag to control spawning

    void Start()
    {
        SpawnEnemies();
        SpawnPotions();
    }

    private void SpawnEnemies()
    {
        int enemySpawnCount = enemyBaseSpawnCount + currentLvl * 10;
        Debug.Log("Spawning Enemies: " + enemySpawnCount);
        for (int i = 0; i < enemySpawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition(enemySpawnRangeX, enemySpawnRangeZ);
            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
    }

    private void SpawnPotions()
    {
        int potionSpawnCount = potionBaseSpawnCount + currentLvl;
        Debug.Log("Spawning Potions: " + potionSpawnCount);
        for (int i = 0; i < potionSpawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition(potionSpawnRangeX, potionSpawnRangeZ);
            Instantiate(healingPotionPrefab, spawnPosition, Quaternion.identity);
        }
    }

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
        }

        // Check if there are no enemies and if we are not currently spawning enemies
        if (GameObject.FindGameObjectsWithTag("Enemy").Length == 0 && !isSpawningEnemies)
        {
            isSpawningEnemies = true;
            levelupUI.SetActive(true);
            StartCoroutine(AllEnemiesKilled());
        }
    }

    IEnumerator AllEnemiesKilled()
    {
        yield return new WaitForSeconds(5);
        levelupUI.SetActive(false);
        currentLvl++;
        Debug.Log("Level Up: " + currentLvl);
        SpawnEnemies();
        SpawnPotions();
        isSpawningEnemies = false; // Reset the flag after spawning
        Debug.Log("EnemySpawn Count: " + (enemyBaseSpawnCount + (currentLvl - 1) * 10));
        Debug.Log("Potion Spawn Count: " + (potionBaseSpawnCount + (currentLvl - 1)));
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }
}
