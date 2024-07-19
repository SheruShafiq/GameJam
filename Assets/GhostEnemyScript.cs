using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GhostEnemyScript : MonoBehaviour
{
    public int maxHP = 10;
    private int currentHP;
    public Slider hpSlider;
    public GameObject deathEffect; // Optional: A particle effect or animation on death

    void Start()
    {
        currentHP = maxHP;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHP;
            hpSlider.value = currentHP;
        }
    }

    void Update()
    {
        // Any additional enemy logic can be added here
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Enemy collided with the player!");
            // You might want to add logic for what happens when the enemy collides with the player
        }

        if (collision.gameObject.CompareTag("QuickAttackProjectile"))
        {
            Debug.Log("Enemy hit by quick attack projectile!");
            TakeDamage(20);
            // Optionally destroy the projectile
            Destroy(collision.gameObject);
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
}
