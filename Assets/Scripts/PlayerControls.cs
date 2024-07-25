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
    private bool throwPotionCooldown = false;
    public GameObject walkingSfx;
    public GameObject quickAttackSfx;
    public GameObject HealingPotionObject;
    public HPBar hpBar;
    public Timer quickAttackTimer;
    public Timer throwPotionTimer;
    private Coroutine quickAttackCooldownCoroutine;
    private Coroutine throwPotionCooldownCoroutine;
    private GameObject throwableObject; // The object to be thrown
    private GameObject replacementObject; // The object to replace the thrown object
    public GameObject firePotion; // The object to be thrown
    public GameObject fireEffect;
    public GameObject electroPotion; // The object to be thrown
    public GameObject electroEffect; // The object to replace the thrown object
    public float throwForce = 10f; // The force with which the object is thrown

    public GameManager gameManager;

    void Start()
    {
        throwableObject = firePotion;
        replacementObject = fireEffect;
        quickAttackCooldown = true;
        throwPotionCooldown = true;
        StartCoroutine(ThrowPotionCoolDown());
        StartCoroutine(QuickAttackCoolDown());
        animator = GetComponent<Animator>();
        if (quickAttackTimer != null)
        {
            quickAttackTimer.onTimerEnd.AddListener(OnQuickAttackCooldownEnd);
        }
        if (throwPotionTimer != null)
        {
            throwPotionTimer.onTimerEnd.AddListener(OnThrowPotionCooldownEnd);
        }
    }

    IEnumerator TurnOffHealingParticleAuraIn4Sec()
    {
        yield return new WaitForSeconds(4);
        onHealingParticleFX.SetActive(false);
    }

    void OnCollisionEnter(Collision collision)
    {
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
                if (throwPotionCooldownCoroutine != null)
                {
                    StopCoroutine(throwPotionCooldownCoroutine);
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
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            throwableObject = firePotion;
            replacementObject = fireEffect;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            throwableObject = electroPotion;
            replacementObject = electroEffect;
        }
        if (hpBar.currentHP <= 0)
        {
            return; // Prevent movement if HP is 0 or less
        }

        if (Input.GetKeyDown(KeyCode.Q) && !quickAttackCooldown)
        {
            PerformQuickAttack();
        }

        if (Input.GetMouseButtonDown(0) && !throwPotionCooldown) // Left mouse button pressed
        {
            PerformThrowPotion();
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

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
        int cooldownDuration = 2;
        if (gameManager.isNukeTriggered)
        {
            cooldownDuration *= 2;
        }
        if (quickAttackTimer != null)
        {
            quickAttackTimer.hours = 0;
            quickAttackTimer.minutes = 0;
            quickAttackTimer.seconds = cooldownDuration;
            quickAttackTimer.StartTimer();
        }

        yield return new WaitForSeconds(cooldownDuration);

        quickAttackCooldown = false;
    }

    void OnQuickAttackCooldownEnd()
    {
        quickAttackCooldown = false;
    }

    void PerformThrowPotion()
    {
        throwPotionCooldown = true;
        animator.SetTrigger("QuickAttack");
        Attacking = true;
        animator.SetBool("isRunning", false);
        animator.SetBool("isWalking", false);
        StartCoroutine(spawnAndThrowObjectCount());

        if (throwPotionCooldownCoroutine != null)
        {
            StopCoroutine(throwPotionCooldownCoroutine);
        }
        throwPotionCooldownCoroutine = StartCoroutine(ThrowPotionCoolDown());
    }

    IEnumerator ThrowPotionCoolDown()
    {
        int cooldownDuration = 3;
        if (gameManager.isNukeTriggered)
        {
            cooldownDuration *= 2;
        }
        if (throwPotionTimer != null)
        {
            throwPotionTimer.hours = 0;
            throwPotionTimer.minutes = 0;
            throwPotionTimer.seconds = cooldownDuration;
            throwPotionTimer.StartTimer();
        }

        yield return new WaitForSeconds(cooldownDuration);

        throwPotionCooldown = false;
    }

    void OnThrowPotionCooldownEnd()
    {
        throwPotionCooldown = false;
    }

    IEnumerator spawnAndThrowObjectCount()
    {
        yield return new WaitForSeconds(0.5f);
        SpawnAndThrowObject();
    }

    public void EndAttack()
    {
        quickAttackBase.SetActive(false);
        Attacking = false;
        quickAttackSfx.SetActive(false);
    }

    void SpawnAndThrowObject()
    {
        if (throwableObject == null)
        {
            Debug.LogError("throwableObject is null!");
            return;
        }

        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 spawnPosition = transform.position; // Spawn at player's position
            GameObject thrownObject = Instantiate(throwableObject, spawnPosition, Quaternion.identity);
            ThrowableObject throwableObjScript = thrownObject.AddComponent<ThrowableObject>();
            throwableObjScript.Initialize(replacementObject);

            // Calculate the throw direction
            Vector3 targetPoint = hit.point;
            Vector3 throwDirection = CalculateThrowDirection(spawnPosition, targetPoint);

            if (!float.IsNaN(throwDirection.x) && !float.IsNaN(throwDirection.y) && !float.IsNaN(throwDirection.z))
            {
                Rigidbody rb = thrownObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddForce(throwDirection, ForceMode.VelocityChange);
                }
                else
                {
                    Debug.LogError("No Rigidbody attached to the throwable object!");
                }
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
        if (replacementObject == null)
        {
            Debug.LogError("replacementObject is null!");
            return;
        }

        Vector3 replacementPosition = transform.position;
        Destroy(gameObject);
        GameObject replacement = Instantiate(replacementObject, replacementPosition, Quaternion.identity);
        Destroy(replacement, 4f); // Destroy the replacement object after 5 seconds
    }
}
