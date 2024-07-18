using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // Reference to the player object
    public Transform player;

    // Offset between the camera and the player
    private Vector3 offset;

    void Start()
    {
        // Calculate the initial offset at the start
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        // Update the camera's position based on the player's position
        Vector3 newPosition = player.position + offset;

        // Keep the Y position of the camera unchanged
        newPosition.y = transform.position.y;

        // Apply the new position to the camera
        transform.position = newPosition;
    }
}
