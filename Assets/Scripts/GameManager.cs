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
    public GameObject BossEnemyV3;

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
    private bool isSpawningEnemies = false;
    public int damageMultiplier = 1;
    public TextMeshProUGUI textMeshProText;
public GameObject gameOverUI;
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
        SpawnBosses();
    }

    private void SpawnBosses()
    {
        if (currentLvl >= 2)
        {
            Instantiate(BossEnemyV2, GetValidSpawnPosition(enemySpawnRangeX + 50, enemySpawnRangeZ + 50), Quaternion.identity);
        }
        if (currentLvl >= 5 )
        {
            for (int i = 0; i < currentLvl - 4; i++)
            {
                Instantiate(BossEnemyV1, GetValidSpawnPosition(enemySpawnRangeX + 50, enemySpawnRangeZ + 50), Quaternion.identity);
            }
        }
        if (currentLvl >= 9)
        {
            Instantiate(BossEnemyV3, GetValidSpawnPosition(enemySpawnRangeX + 50, enemySpawnRangeZ + 50), Quaternion.identity);
        }
        if (currentLvl >=  10)
        {
            for (int i = 0; i < 3; i++)
            {
                Instantiate(BossEnemyV3, GetValidSpawnPosition(enemySpawnRangeX + 50, enemySpawnRangeZ + 50), Quaternion.identity);
            }
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
            tips[currentLvl - 1].SetActive(true);
        }
    }

    public void TriggerNuke()
    {
        isNukeTriggered = true;
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
        currentLvl++;
        if (currentLvl > 10)
        {
            ShowGameOver();
            yield break;
        }
        Debug.Log("Level Up: " + currentLvl);
        SpawnEnemies();
        isSpawningEnemies = false;
        if (textMeshProText)
        {
            textMeshProText.text = "LEVEL " + currentLvl.ToString() + "/10";
        }
    }

    private void ShowGameOver()
    {
        // Implement game over UI logic
        // Assuming you have a game over UI game object, set it active
        // For example:
        // gameOverUI.SetActive(true);
        // You can add more logic here to handle game over state
        Debug.Log("Game Over");
        gameOverUI.SetActive(true);
        defaultHud.SetActive(false);
        // Optionally, load a game over scene or show a game over UI
        // SceneManager.LoadScene("GameOverScene");
    }

    IEnumerator RestartGame()
    {
        yield return new WaitForSeconds(2);
        SceneManager.LoadScene(0);
    }
}
