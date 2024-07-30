using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RangedEnemy : MonoBehaviour
{
    public Animator animator;
    public int maxHP = 500;
    public GameObject fireStatusIcon;
    public GameObject electroStatusIcon;
    private int currentHP;
    public Slider hpSlider;
    public GameObject deathEffect;
    public List<string> currentStatusEffects = new List<string>();
    public float followSpeed = 30f;
    public int damage = 10;
    public float separationDistance = 2f;
    public float separationForce = 5f;
    public float stoppingDistance = 10f; // Changed to a larger value for ranged attack
    public Transform player;
    private GameManager gameManager;
    private Rigidbody rb;
    public GameObject electroVFX;
    public GameObject fireVFX;
    public GameObject nukeVFX;
    private Coroutine electroDamageCoroutine;
    private Coroutine fireDamageCoroutine;
    public HPBar hpBar;
    private bool isHealthCritical;
    public GameObject fireballPrefab; // Fireball prefab to instantiate
    private bool isFiring = false;
    private bool stopFollowing;

    void Start()
    {
        currentHP = maxHP;

        if (hpBar != null)
        {
            hpBar.maxHP = maxHP;
            hpBar.currentHP = currentHP;
            hpBar.UpdateHPDisplay();
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player Transform is not assigned in the Boss script.");
        }

        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager is not assigned in the Boss script.");
        }

        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is not assigned or found in the Boss.");
        }

        StartCoroutine(FollowAndAttackPlayerCoroutine());
    }

    void Update()
    {
        if (hpBar.currentHP <= 100)
        {
            isHealthCritical = true;
        }
        if (hpBar != null)
        {
            hpBar.currentHP = currentHP;
            hpBar.UpdateHPDisplay();
        }

        if (player != null && !gameManager.isPlayerDead)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > stoppingDistance)
            {
                animator.SetBool("isRunning", true);
                FollowPlayer();
            }
            else
            {
                animator.SetBool("isRunning", false);
                if (!isFiring)
                {
                    StartCoroutine(FireballCoroutine());
                }
            }
            ApplySeparation();
        }

        if (gameManager.isPlayerDead)
        {
            animator.SetBool("isRunning", false);
        }

        if (currentStatusEffects.Contains("Fire") && currentStatusEffects.Contains("Electro"))
        {
            HandleNukeEffect();
        }
    }

    void HandleNukeEffect()
    {
        if (nukeVFX != null)
        {
            InstantiateAndDestroyNukeVFX();
            currentStatusEffects.Remove("Fire");
            currentStatusEffects.Remove("Electro");
            fireStatusIcon.SetActive(false);
            electroStatusIcon.SetActive(false);
            electroVFX.SetActive(false);
            fireVFX.SetActive(false);
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

    IEnumerator FollowAndAttackPlayerCoroutine()
    {
        while (true)
        {
            if (player != null && !gameManager.isPlayerDead)
            {
                float distance = Vector3.Distance(transform.position, player.position);
                if (distance <= stoppingDistance && !isFiring)
                {
                    StartCoroutine(FireballCoroutine());
                }
            }
            yield return null;
        }
    }

    IEnumerator FireballCoroutine()
    {
        isFiring = true;
        while (Vector3.Distance(transform.position, player.position) <= stoppingDistance && !gameManager.isPlayerDead)
        {
            animator.SetTrigger("throwFireBall");
            InstantiateFireball();
            
            yield return new WaitForSeconds(1f); // 1 second delay between each fireball
        }
        isFiring = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HealingPotion"))
        {
            StartHealingPotionZoneEffect();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("HealingPotion"))
        {
            StopHealingPotionZoneEffect();
        }
    }

    private void StartHealingPotionZoneEffect()
    {
        InvokeRepeating("HealPlayer", 0, 1);
    }

    private void StopHealingPotionZoneEffect()
    {
        CancelInvoke("HealPlayer");
    }

    void HealPlayer()
    {
        currentHP = Mathf.Min(currentHP + 20, maxHP);
        UpdateHP();
    }

    void FollowPlayer()
    {
        if(!isFiring) {
        float speed = followSpeed;
        Vector3 direction = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, new Vector3(player.position.x, transform.position.y, player.position.z), speed * Time.deltaTime);
        transform.position = newPosition;
        Vector3 playerFace = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(playerFace);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);
        }
    }

    void InstantiateFireball()
    {
        if (fireballPrefab != null)
        {
            Vector3 fireballSpawnPosition = transform.position + transform.forward * 2; // Adjust the spawn position as needed
            Instantiate(fireballPrefab, fireballSpawnPosition, transform.rotation);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EarthPotionInflicter"))
        {
            animator.SetTrigger("takeDamage");
            TakeDamage(500);
        }
        if (collision.gameObject.CompareTag("EarthShatter"))
        {
            animator.SetTrigger("takeDamage");
            TakeDamage(800);
        }
        if (collision.gameObject.CompareTag("Nuke"))
        {
            animator.SetTrigger("takeDamage");
            TakeDamage(100);
            gameManager.TriggerNuke();
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

    public void TakeDamage(int damage)
    {
        currentHP = currentHP - (damage * gameManager.damageMultiplier);
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

    void UpdateHP()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }

        if (hpBar != null)
        {
            hpBar.currentHP = currentHP;
            hpBar.UpdateHPDisplay();
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

    IEnumerator TakeElectroDamageFor5Seconds()
    {
        electroStatusIcon.SetActive(true);
        float timer = 5f;
        electroVFX.SetActive(true);

        while (timer > 0)
        {
            TakeDamage(1);
            yield return new WaitForSeconds(1);
            timer -= 1f;
        }

        electroVFX.SetActive(false);
        currentStatusEffects.Remove("Electro");
        electroStatusIcon.SetActive(false);
        electroDamageCoroutine = null;
    }

    IEnumerator TakeFireDamageFor5Seconds()
    {
        fireStatusIcon.SetActive(true);
        float timer = 5f;
        fireVFX.SetActive(true);

        while (timer > 0)
        {
            TakeDamage(1);
            yield return new WaitForSeconds(1);
            timer -= 1f;
        }

        fireVFX.SetActive(false);
        currentStatusEffects.Remove("Fire");
        fireStatusIcon.SetActive(false);
        fireDamageCoroutine = null;
    }

    void InstantiateAndDestroyNukeVFX()
    {
        GameObject nukeEffect = Instantiate(nukeVFX, transform.position, transform.rotation);
        Destroy(nukeEffect, 2f);
    }
}
