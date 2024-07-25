using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera camera;
    public float moveSpeed = 5f;
    public float knockBackSpeed = 5.0f;
    public float sprintSpeed = 10f;
    public float turnSpeed = 300f;
    public float acceleration = 5f;
    private float currentSpeed;
    private Animator animator;
    public GameObject sprintingSfx;
    public GameObject quickAttackBase;
    public GameObject onHealingParticleFX;
    private bool Attacking = false;
    private bool quickAttackCooldown = false;
    public GameObject walkingSfx;
    public GameObject fireEffect;
    public GameObject quickAttackSfx;
    public GameObject HealingPotionObject;
    public HPBar hpBar;
    public Timer quickAttackTimer;
    private Coroutine quickAttackCooldownCoroutine;
    public GameObject throwableObject; // The object to be thrown
    public GameObject replacementObject; // The object to replace the thrown object
    public float throwForce = 10f; // The force with which the object is thrown

    public GameManager gameManager;

    void Start()
    {
        quickAttackCooldown = true;
        StartCoroutine(QuickAttackCoolDown());
        animator = GetComponent<Animator>();
        if (quickAttackTimer != null)
        {
            quickAttackTimer.onTimerEnd.AddListener(OnQuickAttackCooldownEnd);
        }
    }

    IEnumerator TurnOffHealingParticleAuraIn4Sec()
    {
        yield return new WaitForSeconds(4);
        onHealingParticleFX.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Collided with: " + collision.gameObject.name);
        Debug.Log("Collided with tag: " + collision.gameObject.tag);

        if (collision.gameObject.CompareTag("HealingPotion"))
        {
            StartCoroutine(TurnOffHealingParticleAuraIn4Sec());
            onHealingParticleFX.SetActive(true);
            Destroy(collision.gameObject);
            hpBar.IncreaseHP(20); // Heal the player by 20 points
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Vector3 targetPosition = new Vector3(transform.position.x - 5, transform.position.y, transform.position.z - 5);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, knockBackSpeed * Time.deltaTime);
            Debug.Log("Player collided with an enemy!");
            hpBar.DecreaseHP(10); // Decrease the player's HP by 10 points
            animator.SetTrigger("isHit");
            if (hpBar.currentHP <= 0)
            {
                quickAttackBase.SetActive(false);
                quickAttackSfx.SetActive(false);
                animator.SetBool("isDead", true);
                walkingSfx.SetActive(false);
                sprintingSfx.SetActive(false);
                if (quickAttackCooldownCoroutine != null)
                {
                    StopCoroutine(quickAttackCooldownCoroutine);
                }

                if (gameManager != null)
                {
                    gameManager.isPlayerDead = true;
                }
            }
        }
    }

    void Update()
    {
        if (hpBar.currentHP <= 0)
        {
            return; // Prevent movement if HP is 0 or less
        }

        if (Input.GetKeyDown(KeyCode.Q) && !quickAttackCooldown)
        {
            PerformQuickAttack();
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        if (!Attacking)
        {
            Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

            if (movement.magnitude > 1)
            {
                movement.Normalize();
            }

            bool isMoving = movement.magnitude > 0;
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
                    currentSpeed += acceleration * Time.deltaTime;
                    if (currentSpeed > sprintSpeed)
                    {
                        currentSpeed = sprintSpeed;
                    }
                }
            }
            else
            {
                sprintingSfx.SetActive(false);
                currentSpeed = moveSpeed;
            }

            animator.SetBool("isRunning", isSprinting);
            animator.SetBool("isWalking", isMoving && !isSprinting);

            transform.Translate(currentSpeed * Time.deltaTime * movement, Space.World);

            if (movement != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(movement, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, turnSpeed * Time.deltaTime);
            }
        }

        if (Input.GetMouseButtonDown(0)) // Left mouse button pressed
        {
             animator.SetTrigger("QuickAttack");

            StartCoroutine(spawnandThrowObjectCount());
        }
    }

IEnumerator spawnandThrowObjectCount() {
    yield return new WaitForSeconds(0.5f);
    SpawnAndThrowObject();
}
    void PerformQuickAttack()
    {
        quickAttackCooldown = true;
        quickAttackBase.SetActive(true);
        animator.SetTrigger("QuickAttack");
        Attacking = true;
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        Invoke("EndAttack", 1f);
        quickAttackSfx.SetActive(true);

        if (quickAttackCooldownCoroutine != null)
        {
            StopCoroutine(quickAttackCooldownCoroutine);
        }
        quickAttackCooldownCoroutine = StartCoroutine(QuickAttackCoolDown());
    }

    IEnumerator QuickAttackCoolDown()
    {
        if (quickAttackTimer != null)
        {
            quickAttackTimer.hours = 0;
            quickAttackTimer.minutes = 0;
            quickAttackTimer.seconds = 2;
            quickAttackTimer.StartTimer();
        }

        yield return new WaitForSeconds(2);

        quickAttackCooldown = false;
    }

    void OnQuickAttackCooldownEnd()
    {
        quickAttackCooldown = false;
    }

    public void EndAttack()
    {
        quickAttackBase.SetActive(false);
        Attacking = false;
        quickAttackSfx.SetActive(false);
    }

    void SpawnAndThrowObject()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 spawnPosition = transform.position; // Spawn at player's position
            GameObject thrownObject = Instantiate(throwableObject, spawnPosition, Quaternion.identity);
            thrownObject.AddComponent<ThrowableObject>().Initialize(replacementObject); // Attach the replacement logic to the throwable object

            // Calculate the throw direction
            Vector3 targetPoint = hit.point;
            Vector3 throwDirection = CalculateThrowDirection(spawnPosition, targetPoint);

            if (!float.IsNaN(throwDirection.x) && !float.IsNaN(throwDirection.y) && !float.IsNaN(throwDirection.z))
            {
                Rigidbody rb = thrownObject.GetComponent<Rigidbody>();
                rb.AddForce(throwDirection, ForceMode.VelocityChange);
            }
        }
    }

    Vector3 CalculateThrowDirection(Vector3 start, Vector3 target)
    {
        Vector3 direction = target - start;
        direction.y = 0; // Ignore vertical distance for horizontal throw direction

        float distance = direction.magnitude;
        direction.Normalize();

        float heightDifference = target.y - start.y;

        if (heightDifference < 0)
        {
            heightDifference = 0; // Prevent negative height differences
        }

        // Calculate the initial velocity needed to reach the target
        float initialVelocityY = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * heightDifference);
        float timeToReachTarget = distance / throwForce;

        if (float.IsInfinity(timeToReachTarget) || float.IsNaN(timeToReachTarget) || timeToReachTarget <= 0)
        {
            timeToReachTarget = 1; // Set a default time if the calculation is invalid
        }

        Vector3 velocity = direction * throwForce;
        velocity.y = initialVelocityY;

        return velocity;
    }
}

public class ThrowableObject : MonoBehaviour
{
    private GameObject replacementObject;

    public void Initialize(GameObject replacement)
    {
        replacementObject = replacement;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            ReplaceObject();
        }
    }

    private void ReplaceObject()
    {
        Vector3 replacementPosition = transform.position;
        Destroy(gameObject);
        GameObject replacement = Instantiate(replacementObject, replacementPosition, Quaternion.identity);
        Destroy(replacement, 5f); // Destroy the replacement object after 5 seconds
    }
}
