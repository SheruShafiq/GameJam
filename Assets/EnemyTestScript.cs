using UnityEngine;

public class EnemyTestScript : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 30f; // Speed at which the enemy follows the player

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            // Calculate direction ignoring the Y axis
            Vector3 direction = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;

            // Move towards the player on X and Z axis only
            Vector3 newPosition = Vector3.MoveTowards(transform.position, new Vector3(player.position.x, transform.position.y, player.position.z), followSpeed * Time.deltaTime);
            transform.position = newPosition;

            // Rotate to face the player, might want to adjust rotation to ignore Y axis differences if needed
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * followSpeed);
        }
    }
}