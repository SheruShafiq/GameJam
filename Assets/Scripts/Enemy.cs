using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private bool stopFollowing;
    public Animator animator;
    public int maxHP = 100;
    public GameObject fireStatusIcon;
    public GameObject electroStatusIcon;
    private int currentHP;
    public HPBar hpBar;
    public GameObject deathEffect;
    public List<string> currentStatusEffects = new List<string>();
    public float followSpeed = 30f;
    public int damagePerSecond = 10;
    public float separationDistance = 2f;
    public float separationForce = 5f;
    private Transform player;
    private GameManager gameManager;
    private bool isCollidingWithPlayer = false;
    private GameObject gameManagerObject;
    private Rigidbody rb;
    public GameObject electroVFX;
    public GameObject fireVFX;
    public GameObject nukeVFX;
    private Coroutine electroDamageCoroutine;
    private Coroutine fireDamageCoroutine;
    private Coroutine damageCoroutine;
    private bool isResting = false;
    private bool isHealing = false;

    void Start()
    {
        stopFollowing = true;
        StartCoroutine(ContinueFollowing());
        player = GameObject.FindGameObjectWithTag("Player").transform;
        gameManagerObject = GameObject.FindGameObjectWithTag("GameManager");
        gameManager = gameManagerObject.GetComponent<GameManager>();
        currentHP = maxHP;

        if (hpBar != null)
        {
            hpBar.maxHP = maxHP;
            hpBar.currentHP = currentHP;
            hpBar.UpdateHPDisplay();
        }

        if (player == null)
        {
            Debug.LogError("Player Transform is not assigned in the Enemy script.");
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is not assigned or found in the Enemy.");
        }
    }

    void Update()
    {
        if (player != null && !gameManager.isPlayerDead && !stopFollowing && !isResting)
        {
            FollowPlayer();
            ApplySeparation();
        }

        if (gameManager.isPlayerDead)
        {
            HandlePlayerDeath();
        }

        if (isCollidingWithPlayer && damageCoroutine == null)
        {
            damageCoroutine = StartCoroutine(InflictDamageOverTime());
        }

        if (currentStatusEffects.Contains("Fire") && currentStatusEffects.Contains("Electro"))
        {
            HandleNukeEffect();
        }
    }

    void HandlePlayerDeath()
    {
        animator.SetBool("idle", true);
        FloatAwayFromPlayer();
    }

    void HandleNukeEffect()
    {
        if (nukeVFX != null && GameObject.FindGameObjectsWithTag("Nuke").Length == 0)
        {
            InstantiateAndDestroyNukeVFX();
        }

        if (electroDamageCoroutine != null)
        {
            StopCoroutine(electroDamageCoroutine);
        }

        if (fireDamageCoroutine != null)
        {
            StopCoroutine(fireDamageCoroutine);
        }
    }

    void FollowPlayer()
    {
        Vector3 direction = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, new Vector3(player.position.x, transform.position.y, player.position.z), followSpeed * Time.deltaTime);
        transform.position = newPosition;
        Vector3 playerFace = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(playerFace);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * followSpeed);
    }

    IEnumerator ContinueFollowing()
    {
        yield return new WaitForSeconds(2);
        stopFollowing = false;
    }

    void InstantiateAndDestroyNukeVFX()
    {
        GameObject nukeEffect = Instantiate(nukeVFX, transform.position, transform.rotation);
        Destroy(nukeEffect, 2f);
    }

    IEnumerator TakeElectroDamageFor5Seconds()
    {
        electroStatusIcon.SetActive(true);
        float timer = 3f;
        electroVFX.SetActive(true);

        while (timer > 0)
        {
            TakeDamage(10);
            yield return new WaitForSeconds(1);
            timer -= 1f;
        }

        electroVFX.SetActive(false);
        currentStatusEffects.Remove("Electro");
        electroStatusIcon.SetActive(false);
        electroDamageCoroutine = null;
    }

    IEnumerator InflictDamageOverTime()
    {
        while (isCollidingWithPlayer)
        {
            AttackPlayer();
            yield return new WaitForSeconds(1);
        }
        damageCoroutine = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HealingPotion"))
        {
            if (hpBar != null && !isHealing)
            {
                HealingPotionZoneEffect();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("HealingPotion"))
        {
            StopHealingPotionZoneEffect();
        }
    }

    private void HealingPotionZoneEffect()
    {
        isHealing = true;
        InvokeRepeating("HealPlayer", 0, 1);
    }

    private void StopHealingPotionZoneEffect()
    {
        isHealing = false;
        CancelInvoke("HealPlayer");
    }

    void HealPlayer()
    {
        if (hpBar != null)
        {
            hpBar.currentHP = Mathf.Min(hpBar.currentHP + 20, hpBar.maxHP);
            hpBar.UpdateHPDisplay();
        }
    }

    void AttackPlayer()
    {
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null && !playerController.currentStatusEffects.Contains("Earth"))
        {
            playerController.hpBar.DecreaseHP(damagePerSecond);
        }
        else
        {
            Debug.LogError("PlayerController component not found on the player.");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Nuke"))
        {
            TakeDamage(100);
            if (currentHP <= 0)
            {
                gameManager.TriggerNuke();
                Die();
            }
        }
        else if (collision.gameObject.CompareTag("EarthPotionInflicter"))
        {
            Die();
        }
        else if (collision.gameObject.CompareTag("EarthShatter"))
        {
            Die();
        }
        else if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = true;
        }
        else if (collision.gameObject.CompareTag("QuickAttackProjectile"))
        {
            TakeDamage(50);
        }
        else if (collision.gameObject.CompareTag("Lightning Effect Inflicter"))
        {
            if (electroDamageCoroutine != null)
            {
                StopCoroutine(electroDamageCoroutine);
            }
            currentStatusEffects.Add("Electro");
            electroDamageCoroutine = StartCoroutine(TakeElectroDamageFor5Seconds());
        }
        else if (collision.gameObject.CompareTag("Fire Effect Inflicter"))
        {
            if (fireDamageCoroutine != null)
            {
                StopCoroutine(fireDamageCoroutine);
            }
            currentStatusEffects.Add("Fire");
            fireDamageCoroutine = StartCoroutine(TakeFireDamageFor5Seconds());
        }
    }

    IEnumerator TakeFireDamageFor5Seconds()
    {
        fireStatusIcon.SetActive(true);
        float timer = 3f;
        fireVFX.SetActive(true);

        while (timer > 0)
        {
            TakeDamage(10);
            yield return new WaitForSeconds(1);
            timer -= 1f;
        }

        fireVFX.SetActive(false);
        currentStatusEffects.Remove("Fire");
        fireStatusIcon.SetActive(false);
        fireDamageCoroutine = null;
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = false;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage * gameManager.damageMultiplier;
        if (hpBar != null)
        {
            hpBar.currentHP = currentHP;
            hpBar.UpdateHPDisplay();
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathEffect != null)
        {
            GameObject effect = Instantiate(deathEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
        }

        Destroy(gameObject);
    }

    void FloatAwayFromPlayer()
    {
        if (rb != null && player != null)
        {
            Vector3 pushDirection = (transform.position - player.position).normalized;
            rb.AddForce(pushDirection * 10f, ForceMode.Impulse);
        }
    }

    void ApplySeparation()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, separationDistance);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject != gameObject && collider.CompareTag("Enemy"))
            {
                Vector3 separationDirection = transform.position - collider.transform.position;
                rb.AddForce(separationDirection.normalized * separationForce, ForceMode.Force);
            }
        }
    }
}
