using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    public GameManager gameManager;
    private bool stopFollowing;
    public Animator animator;
    public int maxHP = 10;
    private int currentHP;
    public Slider hpSlider;
    public GameObject deathEffect; // Optional: A particle effect or animation on death

    public Transform player;
    public float followSpeed = 30f; // Speed at which the enemy follows the player
    public int damagePerSecond = 10; // Damage inflicted per second while in collision
    public float separationDistance = 2f; // Minimum distance between enemies to avoid merging
    public float separationForce = 5f; // Force applied to separate enemies

    private bool isCollidingWithPlayer = false;
    private Rigidbody rb; // Add a reference to the Rigidbody

    void Start()
    {
        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
        if (player == null)
        {
            Debug.LogError("Player Transform is not assigned in the Enemy script.");
        }

        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component is not assigned or found in the Enemy.");
        }
    }

    IEnumerator continueFowllowing()
    {
        yield return new WaitForSeconds(2);
        stopFollowing = false;
    }

    void Update()
    {
        if (player != null && !gameManager.isPlayerDead && !stopFollowing)
        {
            // Calculate direction ignoring the Y axis
            Vector3 direction = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;

            // Move towards the player on X and Z axis only
            Vector3 newPosition = Vector3.MoveTowards(transform.position, new Vector3(player.position.x, transform.position.y, player.position.z), followSpeed * Time.deltaTime);
            transform.position = newPosition;
            Vector3 playerFace = (player.position - transform.position).normalized;
            // Rotate to face the player, might want to adjust rotation to ignore Y axis differences if needed
            Quaternion lookRotation = Quaternion.LookRotation(playerFace);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * followSpeed);

            // Apply separation force
            ApplySeparation();
        }
        if (gameManager.isPlayerDead)
        {
            animator.SetBool("idle", true);
            FloatAwayFromPlayer();
        }
        if (isCollidingWithPlayer)
        {
            // Inflict damage and play attack animation
            InflictDamage();
        }
    }

    void InflictDamage()
    {
        // Assuming there's a method in PlayerController to decrease HP
        var playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.hpBar.DecreaseHP(Mathf.FloorToInt(damagePerSecond * Time.deltaTime));
            stopFollowing = true;
            StartCoroutine(continueFowllowing());
        }
        else
        {
            Debug.LogError("PlayerController component not found on the player.");
        }
    }

   void OnCollisionEnter(Collision collision)
{
    if (collision.gameObject.CompareTag("Player"))
    {
        isCollidingWithPlayer = true;
    }

    if (collision.gameObject.CompareTag("QuickAttackProjectile"))
    {
        TakeDamage(20);
        // Destroy the entity this script is attached to
        Destroy(gameObject);
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
        currentHP -= damage;
        if (hpSlider != null)
        {
            hpSlider.value = currentHP;
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Enemy died!");
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }

    void FloatAwayFromPlayer()
    {
        if (rb != null && player != null)
        {
            Vector3 pushDirection = (transform.position - player.position).normalized;
            rb.AddForce(pushDirection * 10f, ForceMode.Impulse); // Adjust the force value as needed
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