using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Prefabs")]
    public bool isNukeTriggered = false;
    public GameObject levelupUI;
    public GameObject BossEnemyV1;
    public GameObject BossEnemyV2;

    // public GameObject tutorialText;
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
    public GameObject nukeTriggeredUi;
    public int currentLvl = 1;
    private bool isSpawningEnemies = false; // Flag to control spawning
    public int damageMultiplier = 1;
     public TextMeshProUGUI textMeshProText;

    public GameObject[] tips;

    void Start()
    {
        SpawnEnemies();
        if (!textMeshProText && GetComponent<TextMeshProUGUI>())
        {
            textMeshProText = GetComponent<TextMeshProUGUI>();
        }
        textMeshProText.text = "LEVEL " + currentLvl.ToString() + "/10";
    }

    private void SpawnEnemies()
    {
        int enemySpawnCount = enemyBaseSpawnCount + currentLvl * 10;
        Debug.Log("Spawning Enemies: " + enemySpawnCount);
        for (int i = 0; i < enemySpawnCount; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition(enemySpawnRangeX + 10, enemySpawnRangeZ + 10);

            Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        }
        SpawnBoss();
        SpawnBossV2 ();
    }

    private void SpawnBoss()
    {
        if (currentLvl > 5)
            for (int i = 0; i < currentLvl * 2; i++)
            {
                Instantiate(BossEnemyV1, GetValidSpawnPosition(enemySpawnRangeX + 50, enemySpawnRangeZ + 50), Quaternion.identity);
            }
    }
    private void SpawnBossV2()
    {
        if (currentLvl > 3)
            for (int i = 0; i < currentLvl * 2; i++)
            {
                Instantiate(BossEnemyV2, GetValidSpawnPosition(enemySpawnRangeX + 50, enemySpawnRangeZ + 50), Quaternion.identity);
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

    private Vector3 GetValidSpawnPosition(float rangeX, float rangeZ)
    {
        Vector3 spawnPosition;
        do
        {
            spawnPosition = GetRandomSpawnPosition(rangeX, rangeZ);
        } while (Mathf.Abs(spawnPosition.x - playerTransform.position.x) < 20 || Mathf.Abs(spawnPosition.z - playerTransform.position.z) < 20);

        return spawnPosition;
    }

    void Update()
    {
        if (isNukeTriggered)
        {
            nukeTriggeredUi.SetActive(true);
        }
        else
        {
            nukeTriggeredUi.SetActive(false);
        }
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
            // tutorialText.SetActive(false);
            levelupUI.SetActive(true);
            StartCoroutine(AllEnemiesKilled());
            tips[currentLvl - 1].SetActive(true);
        }
    }

    public void TriggerNuke()
    {
        isNukeTriggered = true;
        //  StartCoroutine(resetNuke());
    }

    IEnumerator resetNuke()
    {
        yield return new WaitForSeconds(4);
        isNukeTriggered = false;
    }
    IEnumerator AllEnemiesKilled()
    {
        yield return new WaitForSeconds(5);
        levelupUI.SetActive(false);
        tips[currentLvl - 1].SetActive(false);
        // tutorialText.SetActive(true);
        currentLvl++;
        Debug.Log("Level Up: " + currentLvl);
        SpawnEnemies();
        // SpawnPotions();
        isSpawningEnemies = false; // Reset the flag after spawning
        Debug.Log("EnemySpawn Count: " + (enemyBaseSpawnCount + (currentLvl - 1) * 10));
      //  Debug.Log("Potion Spawn Count: " + (potionBaseSpawnCount + (currentLvl - 1)));
        if (textMeshProText)
        {
            textMeshProText.text = "LEVEL " + currentLvl.ToString() + "/10";
        }
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }
}