using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;       // Speed of movement
    public float sprintSpeed = 10f;    // Speed of movement while sprinting
    public float turnSpeed = 300f;     // Speed of turning
    public float acceleration = 5f; // Adjust this value to control how quickly the speed increases
    private float currentSpeed;
    private Animator animator;
    public GameObject sprintingSfx;
    public GameObject quickAttackParticleFX;
    private bool Attacking = false;
    public GameObject walkingSfx;
    public GameObject quickAttackSfx;
    void Start()
    {
        // Get the Animator component attached to the player
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            quickAttackParticleFX.SetActive(true);
            animator.SetTrigger("QuickAttack");
            Attacking = true;
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", false);
            Invoke("EndAttack", 1f);
            quickAttackSfx.SetActive(true);

        }
        // Get input from WASD keys
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        if (!Attacking)
        {
            // Calculate movement direction
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

            // Normalize the movement vector to ensure consistent movement speed in all directions
            if (movement.magnitude > 1)
            {
                movement.Normalize();
            }

            // Determine if the player is moving
            bool isMoving = movement.magnitude > 0;

            // Check if the sprint key (Left Shift) is pressed
            bool isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;
            if (isMoving)
            {
                walkingSfx.SetActive(true);
            }
            else
            {
                walkingSfx.SetActive(false);
            }
            if (isSprinting)
            {
                walkingSfx.SetActive(false);
                sprintingSfx.SetActive(true);
                if (currentSpeed < sprintSpeed)
                {
                    currentSpeed += acceleration * Time.deltaTime; // Gradually increase speed
                    if (currentSpeed > sprintSpeed)
                    {
                        currentSpeed = sprintSpeed; // Ensure currentSpeed does not exceed sprintSpeed
                    }
                }
            }
            else
            {
                sprintingSfx.SetActive(false);

                currentSpeed = moveSpeed;
            }

            // Set animation parameters
            animator.SetBool("isRunning", isSprinting);
            animator.SetBool("isWalking", isMoving && !isSprinting);

            // Move the player
            transform.Translate(currentSpeed * Time.deltaTime * movement, Space.World);

            // If there is some movement, rotate the player to face the movement direction
            if (movement != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
            }
        }
    }
    public void EndAttack()
    {
        quickAttackParticleFX.SetActive(false);
        Attacking = false;
        quickAttackSfx.SetActive(false);

    }
}
