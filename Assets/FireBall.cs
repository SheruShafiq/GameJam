using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 20;
    private Transform player;
    private Vector3 targetPosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player Transform is not assigned in the Fireball script.");
            Destroy(gameObject);
            return;
        }
        targetPosition = player.position;
        StartCoroutine(ScaleDownCoroutine());
    }
   private IEnumerator ScaleDownCoroutine()
    {
        Vector3 originalScale = transform.localScale;
        float duration = 3f; // Duration in seconds
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float progress = elapsedTime / duration;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the scale is set to zero at the end
        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }
//keep sizing down to 0 and then destroy
    void Update()
    {
        if (player != null)
        {
            // Move fireball towards the player's current position
            targetPosition = player.position;
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.position += direction * speed * Time.deltaTime;
        }
        
        // Optionally, destroy the fireball if it gets too far away or doesn't hit the player within a certain time
        // Destroy(gameObject, 5f); // Destroy after 5 seconds if it doesn't hit anything
    }

    void OnTriggerEnter(Collider other)
    {
        // if (other.CompareTag("Player"))
        // {
        //     PlayerController playerController = other.GetComponent<PlayerController>();
        //     if (playerController != null && !playerController.currentStatusEffects.Contains("earth"))
        //     {
        //         playerController.hpBar.DecreaseHP(damage);
        //     }

        //     // Optionally, add an effect on impact
        //     // Instantiate(impactEffect, transform.position, transform.rotation);

        //     Destroy(gameObject); // Destroy the fireball on impact
        // }
        // else 
        // {
            Destroy(gameObject); // Destroy the fireball if it hits an obstacle
        // }
    }
}
