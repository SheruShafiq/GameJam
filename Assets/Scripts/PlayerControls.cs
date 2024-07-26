using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public UISelectedFrameScript uiElement;
    public Transform target1;
    private Animator animator;
    public GameObject sprintingSfx;
    public GameObject quickAttackBase;
    public GameObject onHealingParticleFX;
    private bool Attacking = false;
    private bool quickAttackCooldown = false;
    private bool throwPotionCooldown = false;
    public GameObject selectedPotionUI;
    public GameObject walkingSfx;
    public GameObject quickAttackSfx;
    public GameObject HealingPotionObject;
    public HPBar hpBar;
    public Timer quickAttackTimer;
    public Timer throwPotionTimer;
    private Coroutine quickAttackCooldownCoroutine;
    private Coroutine throwPotionCooldownCoroutine;
    private GameObject throwableObject;
    private GameObject replacementObject;
    public GameObject firePotion;
    public GameObject fireEffect;
    public GameObject electroPotion;
    public GameObject electroEffect;
    public float throwForce = 10f;
    private Animator uiAnimator;
    public GameManager gameManager;

    public GameObject fireStatusIcon;
    public GameObject electroStatusIcon;

    public GameObject fireVFX;
    public GameObject electroVFX;
    public GameObject nukeVFX;
    public List<string> currentStatusEffects = new List<string>();
    private Coroutine fireDamageCoroutine;
    private Coroutine electroDamageCoroutine;
    private bool isSprinting;

    void Start()
    {
        if (hpBar != null)
        {
            hpBar.maxHP = 100;
            hpBar.currentHP = hpBar.maxHP;
            hpBar.UpdateHPDisplay();
        }
        uiAnimator = selectedPotionUI.GetComponent<Animator>();
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

    void Update()
    {
        hpBar.UpdateHPDisplay();
        if (hpBar.currentHP <= 0)
        {
            HandleDeath();
            return;
        }
        

        if (currentStatusEffects.Contains("Fire") && currentStatusEffects.Contains("Electro"))
        {
            if (nukeVFX != null && GameObject.FindGameObjectsWithTag("Nuke").Length == 0)
            {
                InstantiateAndDestroyNukeVFX();
            }

            if (electroDamageCoroutine != null)
            {
                StopCoroutine(electroDamageCoroutine);
            }

            if (fireDamageCoroutine != null)
            {
                StopCoroutine(fireDamageCoroutine);
            }
            currentStatusEffects.Remove("Fire");
            currentStatusEffects.Remove("Electro");
            fireStatusIcon.SetActive(false);
            electroStatusIcon.SetActive(false);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (throwableObject == firePotion && replacementObject == fireEffect)
            {
                return;
            }
            uiAnimator.SetBool("onClick2", false);
            uiAnimator.SetBool("onClick1", true);
            throwableObject = firePotion;
            replacementObject = fireEffect;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (throwableObject == electroPotion && replacementObject == electroEffect)
            {
                return;
            }
            uiAnimator.SetBool("onClick1", false);
            uiAnimator.SetBool("onClick2", true);
            throwableObject = electroPotion;
            replacementObject = electroEffect;
        }

        if (Input.GetKeyDown(KeyCode.Q) && !quickAttackCooldown)
        {
            PerformQuickAttack();
        }

        if (Input.GetMouseButtonDown(0) && !throwPotionCooldown)
        {
            PerformThrowPotion();
        }

        if (Input.GetMouseButton(1))
        {
            PerformDrinkPotion();
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        if (movement.magnitude > 1)
        {
            movement.Normalize();
        }

        bool isMoving = movement.magnitude > 0;
         isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving;
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

    void HandleDeath()
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

    void InstantiateAndDestroyNukeVFX()
    {
        if (gameManager.isNukeTriggered)
        {
            return;
        }

        GameObject nukeEffect = Instantiate(nukeVFX, transform.position, transform.rotation);
        TakeDamage(90);
        Destroy(nukeEffect, 2f);
        hpBar.UpdateHPDisplay();
        gameManager.isNukeTriggered = true;
        currentStatusEffects.Remove("Fire");
        currentStatusEffects.Remove("Electro");
        fireStatusIcon.SetActive(false);
        electroStatusIcon.SetActive(false);
        fireVFX.SetActive(false);
        electroVFX.SetActive(false);
        StopCoroutine(fireDamageCoroutine);
        StopCoroutine(electroDamageCoroutine);
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
            cooldownDuration *= 5;
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

    void PerformDrinkPotion()
    {
        if (throwableObject == firePotion)
        {
            ApplyFireEffect();
        }
        else if (throwableObject == electroPotion)
        {
            ApplyElectroEffect();
        }
    }

    void ApplyFireEffect()
    {
        if (!currentStatusEffects.Contains("Fire"))
        {
            currentStatusEffects.Add("Fire");
            fireDamageCoroutine = StartCoroutine(TakeFireDamageFor5Seconds());
        }
    }

    void ApplyElectroEffect()
    {
        if (!currentStatusEffects.Contains("Electro") && !gameManager.isNukeTriggered)
        {
            currentStatusEffects.Add("Electro");
            electroDamageCoroutine = StartCoroutine(ElectroStatusEffect());
        }
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

    IEnumerator ElectroStatusEffect()
    {
        if (electroDamageCoroutine != null)
        {
            StopCoroutine(electroDamageCoroutine);
        }

        electroStatusIcon.SetActive(true);
        float timer = 5f;
        electroVFX.SetActive(true);

        sprintSpeed *= 2;
        turnSpeed *= 2;
        acceleration *= 2;

        while (timer > 0)
        {
            TakeDamage(1);
            yield return new WaitForSeconds(1);
            timer -= 1f;
        }

        sprintSpeed = 10f;
        turnSpeed = 300f;
        acceleration = 5f;
isSprinting = false;
        electroVFX.SetActive(false);
        currentStatusEffects.Remove("Electro");
        electroStatusIcon.SetActive(false);
        electroDamageCoroutine = null;
    }

    public void TakeDamage(int damage)
    {
        if (hpBar != null)
        {
            hpBar.DecreaseHP(damage);
            hpBar.UpdateHPDisplay();
        }
    }

    IEnumerator ThrowPotionCoolDown()
    {
        int cooldownDuration = 2;
        if (gameManager.isNukeTriggered)
        {
            cooldownDuration *= 2;
            gameManager.isNukeTriggered = false;
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
            Vector3 spawnPosition = transform.position;
            GameObject thrownObject = Instantiate(throwableObject, spawnPosition, Quaternion.identity);
            ThrowableObject throwableObjScript = thrownObject.AddComponent<ThrowableObject>();
            throwableObjScript.Initialize(replacementObject);

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
        direction.y = 0;

        float distance = direction.magnitude;
        direction.Normalize();

        float heightDifference = target.y - start.y;

        if (heightDifference < 0)
        {
            heightDifference = 0;
        }

        float initialVelocityY = Mathf.Sqrt(2 * Mathf.Abs(Physics.gravity.y) * heightDifference);
        float timeToReachTarget = distance / throwForce;

        if (float.IsInfinity(timeToReachTarget) || float.IsNaN(timeToReachTarget) || timeToReachTarget <= 0)
        {
            timeToReachTarget = 1;
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
        Destroy(replacement, 4f);
    }
}
