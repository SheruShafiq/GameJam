using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossEnemyV1 : MonoBehaviour
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
    public float stoppingDistance = 2f;
    public Transform player;
    private GameManager gameManager;
    private bool isCollidingWithPlayer = false;
    private Rigidbody rb;
    public GameObject electroVFX;
    public GameObject fireVFX;
    public GameObject nukeVFX;
    private Coroutine electroDamageCoroutine;
    private Coroutine fireDamageCoroutine;
    private bool isResting = false;
    private bool stopFollowing = false;
    public HPBar hpBar;
    private bool isHealing = false;
    private bool isHealthCritical;

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

        StartCoroutine(FollowPlayerCoroutine());
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

        if (isResting)
        {
            animator.SetBool("isRunning", false);
        }

        if (player != null && !gameManager.isPlayerDead && !isResting)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance > stoppingDistance && !stopFollowing)
            {
                animator.SetBool("isRunning", true);
                FollowPlayer();
            }
            else
            {
                animator.SetBool("isRunning", false);
            }
            ApplySeparation();
        }

        if (gameManager.isPlayerDead)
        {
            PermanentlyRest();
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

    IEnumerator FollowPlayerCoroutine()
    {
        while (true)
        {
            if (isCollidingWithPlayer)
            {
                AttackPlayer();
                yield return new WaitForSeconds(1);
                isResting = true;
                animator.SetBool("isResting", true);

                yield return new WaitForSeconds(3);
                isResting = false;
                animator.SetBool("isResting", false);
            }
            yield return null;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HealingPotion"))
        {
            if (!isHealing)
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
        currentHP = Mathf.Min(currentHP + 20, maxHP);
        UpdateHP();
    }

    void FollowPlayer()
    {
        float speed = followSpeed;
        Vector3 direction = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;
        Vector3 newPosition = Vector3.MoveTowards(transform.position, new Vector3(player.position.x, transform.position.y, player.position.z), speed * Time.deltaTime);
        transform.position = newPosition;
        Vector3 playerFace = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(playerFace);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * speed);
    }

    void AttackPlayer()
    {
        animator.SetBool("isRunning", false);
        animator.SetTrigger("attack");
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null && !playerController.currentStatusEffects.Contains("Earth"))
        {
            playerController.hpBar.DecreaseHP(damage);
        }
    }

    void PermanentlyRest()
    {
        isResting = true;
        animator.SetBool("isResting", true);
        StopAllCoroutines();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("EarthPotionInflicter"))
        {
            animator.SetTrigger("takeDamage");
            TakeDamage(300);
            stopFollowing = true;
            StartCoroutine(ContinueFollowing());
        }
        if (collision.gameObject.CompareTag("EarthShatter"))
        {
            animator.SetTrigger("takeDamage");
            TakeDamage(500);
            stopFollowing = true;
            StartCoroutine(ContinueFollowing());

        }
        if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = true;
        }
        else if (collision.gameObject.CompareTag("Nuke"))
        {
            stopFollowing = true;
            StartCoroutine(ContinueFollowing());

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

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isCollidingWithPlayer = false;
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

    IEnumerator ContinueFollowing()
    {
        yield return new WaitForSeconds(1);
        stopFollowing = false;
    }
}
