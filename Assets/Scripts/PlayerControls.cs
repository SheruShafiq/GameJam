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
    public GameObject HealingPotionObject;  // New Healing Potion object
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
    public GameObject healingVFX;
    private bool isHealing = false;


    void Start()
    {
        if (hpBar != null)
        {
            hpBar.maxHP = 100;
            hpBar.currentHP = hpBar.maxHP;
            hpBar.UpdateHPDisplay();
        }

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
        // if (!currentStatusEffects.Contains("Electro"))
        // {
        //     StopCoroutine(electroDamageCoroutine);
        // }
        // if (!currentStatusEffects.Contains("Fire"))
        // {
        //     StopCoroutine(fireDamageCoroutine);
        // }

        hpBar.UpdateHPDisplay();
        if (hpBar.currentHP <= 0)
        {
            HandleDeath();
            return;
        }

        if (currentStatusEffects.Contains("Fire") && currentStatusEffects.Contains("Electro"))
        {
            currentStatusEffects.Remove("Fire");
            currentStatusEffects.Remove("Electro");
            if (nukeVFX != null && GameObject.FindGameObjectsWithTag("Nuke").Length == 0)
            {
                InstantiateAndDestroyNukeVFX();
            }


            currentStatusEffects.Remove("Fire");
            currentStatusEffects.Remove("Electro");
            fireStatusIcon.SetActive(false);
            electroStatusIcon.SetActive(false);

            fireVFX.SetActive(false);
            electroVFX.SetActive(false);


        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (throwableObject == firePotion && replacementObject == fireEffect)
            {
                return;
            }
            throwableObject = firePotion;
            replacementObject = fireEffect;
            selectedPotionUI.transform.position = new Vector3(140, selectedPotionUI.transform.position.y, selectedPotionUI.transform.position.z);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (throwableObject == electroPotion && replacementObject == electroEffect)
            {
                return;
            }
            throwableObject = electroPotion;
            replacementObject = electroEffect;
            selectedPotionUI.transform.position = new Vector3(280, selectedPotionUI.transform.position.y, selectedPotionUI.transform.position.z);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            if (throwableObject == HealingPotionObject)
            {
                return;
            }
            throwableObject = HealingPotionObject;
            replacementObject = healingVFX;  // No replacement effect for healing potion
            selectedPotionUI.transform.position = new Vector3(420, selectedPotionUI.transform.position.y, selectedPotionUI.transform.position.z);
        }

        if (Input.GetKeyDown(KeyCode.Q) && !quickAttackCooldown)
        {
            PerformQuickAttack();
        }

        if (Input.GetMouseButtonDown(0) && !throwPotionCooldown)
        {
            PerformThrowPotion();
        }

        if (Input.GetMouseButton(1) && !throwPotionCooldown)
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
            currentSpeed = Mathf.Min(currentSpeed + acceleration * Time.deltaTime, sprintSpeed);
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
    // on collision enter with healing potion
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("HealingPotion"))
        {
            if (hpBar != null && !isHealing)
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


    void PerformDrinkPotion()
    {
        if (!throwPotionCooldown)
        {
            if (throwableObject == firePotion)
            {
                ApplyFireEffect();
            }
            else if (throwableObject == electroPotion)
            {
                ApplyElectroEffect();
            }
            else if (throwableObject == HealingPotionObject)
            {
                HealPlayerDrink();
            }

            throwPotionCooldown = true;
            if (throwPotionCooldownCoroutine != null)
            {
                StopCoroutine(throwPotionCooldownCoroutine);
            }
            throwPotionCooldownCoroutine = StartCoroutine(ThrowPotionCoolDown());
        }
    }


    void HealPlayer()
    {
        if (hpBar != null)
        {
            hpBar.currentHP = Mathf.Min(hpBar.currentHP + 20, hpBar.maxHP);
            hpBar.UpdateHPDisplay();
            // currentStatusEffects.Add("Healing");
        }

    }
    void HealPlayerDrink()
    {
        if (hpBar != null)
        {
            hpBar.currentHP = Mathf.Min(hpBar.currentHP + 10, hpBar.maxHP);
            hpBar.UpdateHPDisplay();
            // currentStatusEffects.Add("Healing");
        }

    }

    void ApplyFireEffect()
    {
        if (!currentStatusEffects.Contains("Fire"))
        {
            currentStatusEffects.Add("Fire");
            fireDamageCoroutine = StartCoroutine(FireStatusEffect());
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

    IEnumerator FireStatusEffect()
    {
        fireStatusIcon.SetActive(true);
        float timer = 5f;
        fireVFX.SetActive(true);
        gameManager.damageMultiplier = 2;
        while (timer > 0)
        {
            TakeDamage(5);
            yield return new WaitForSeconds(1);
            timer -= 1f;
        }
        gameManager.damageMultiplier = 1;
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

        float originalSprintSpeed = sprintSpeed;
        float originalTurnSpeed = turnSpeed;
        float originalAcceleration = acceleration;

        sprintSpeed *= 2;
        turnSpeed *= 2;
        acceleration *= 2;

        while (timer > 0)
        {
            TakeDamage(1);
            yield return new WaitForSeconds(1);
            timer -= 1f;
        }

        sprintSpeed = originalSprintSpeed;
        turnSpeed = 1000;
        acceleration = originalAcceleration;

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
