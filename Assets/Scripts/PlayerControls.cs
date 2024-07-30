using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Camera camera;
    public float moveSpeed = 5f;
    public float knockBackSpeed = 5.0f;
    public float sprintSpeed = 10f;
    public float turnSpeed = 300f;
    public float acceleration = 5f;
    public GameObject healingPotionFrame;
    public GameObject firePotionFrame;
    public GameObject electroPotionFrame;
    public GameObject earthPotionFrame;
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
    public GameObject earthPotion;
    public GameObject earthEffect;
    public float throwForce = 10f;

    public GameManager gameManager;
    public GameObject fireStatusIcon;
    public GameObject electroStatusIcon;
    public GameObject earthStatusIcon;
    public GameObject fireVFX;
    public GameObject electroVFX;
    public GameObject earthVFX;
    public GameObject nukeVFX;
    public List<string> currentStatusEffects = new List<string>();

    private Coroutine electroDamageCoroutine;
    private Coroutine earthStatusEffectCoroutine;
    private bool isSprinting;
    public GameObject healingVFX;
    private bool isHealing = false;
    private Coroutine fireStatusEffectCoroutine;
    public GameObject healingPlayerFX;
    public GameObject EarthShatterFX;
    private int earthDamagePreventionCount = 0;
    private bool isInvincible;

    private float currentSpeed;
    public GameObject drinkSfx;

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
        firePotionFrame.SetActive(true);
        quickAttackCooldownCoroutine = StartCoroutine(QuickAttackCoolDown());
        throwPotionCooldownCoroutine = StartCoroutine(ThrowPotionCoolDown());
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
 private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("FireBall") && !currentStatusEffects.Contains("Earth")) 
        {
            TakeDamage(10);
            ApplyFireEffect();
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
            HandleNukeEffect();
        }

        HandlePotionSelection();
        HandleInput();
        HandleMovement();
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
            quickAttackCooldownCoroutine = null;
        }
        if (throwPotionCooldownCoroutine != null)
        {
            StopCoroutine(throwPotionCooldownCoroutine);
            throwPotionCooldownCoroutine = null;
        }
        if (gameManager != null)
        {
            gameManager.isPlayerDead = true;
        }
        moveSpeed = 0;
        sprintSpeed = 0;
        turnSpeed = 0;
        acceleration = 0;
    }

    void HandleNukeEffect()
    {
        currentStatusEffects.Remove("Fire");
        currentStatusEffects.Remove("Electro");
        if (nukeVFX != null && GameObject.FindGameObjectsWithTag("Nuke").Length == 0)
        {
            InstantiateAndDestroyNukeVFX();
        }
        fireStatusIcon.SetActive(false);
        electroStatusIcon.SetActive(false);
        fireVFX.SetActive(false);
        electroVFX.SetActive(false);
    }

    void HandlePotionSelection()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SelectPotion(firePotion, fireEffect, firePotionFrame);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SelectPotion(electroPotion, electroEffect, electroPotionFrame);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SelectPotion(HealingPotionObject, healingVFX, healingPotionFrame);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SelectPotion(earthPotion, earthEffect, earthPotionFrame);
            if (currentStatusEffects.Contains("Fire"))
            {
                replacementObject = EarthShatterFX;
            }
        }
    }

    void SelectPotion(GameObject potion, GameObject effect, GameObject frame)
    {
        if (throwableObject == potion && replacementObject == effect)
        {
            return;
        }
        firePotionFrame.SetActive(false);
        electroPotionFrame.SetActive(false);
        healingPotionFrame.SetActive(false);
        earthPotionFrame.SetActive(false);
        frame.SetActive(true);
        throwableObject = potion;
        replacementObject = effect;
    }

    void HandleInput()
    {
        // if (Input.GetKeyDown(KeyCode.Q) && !quickAttackCooldown)
        // {
        //     PerformQuickAttack();
        // }

        if (Input.GetMouseButtonDown(0) && !throwPotionCooldown)
        {
            PerformThrowPotion();
        }

        if (Input.GetMouseButton(1) && !throwPotionCooldown ) {
           
            PerformDrinkPotion();
        }
        if( Input.GetKeyDown(KeyCode.E)&& !throwPotionCooldown ) {
             PerformDrinkPotion();
            
        }
    }

    void HandleMovement()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if (!gameManager.isPlayerDead)
        {
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


        if (throwableObject == firePotion)
        {
            drinkSfx.SetActive(true);
            ApplyFireEffect();
            StartCoroutine(resetDrinkSfx());
        }
        else if (throwableObject == electroPotion)
        {
            drinkSfx.SetActive(true);
            StartCoroutine(resetDrinkSfx());

            ApplyElectroEffect();
        }
        else if (throwableObject == HealingPotionObject)
        {
            drinkSfx.SetActive(true);
            StartCoroutine(resetDrinkSfx());


            HealPlayerDrink();
        }
        else if (throwableObject == earthPotion)
        {
            drinkSfx.SetActive(true);
            StartCoroutine(resetDrinkSfx());


            ApplyEarthEffect();
        }

        throwPotionCooldown = true;
        if (throwPotionCooldownCoroutine != null)
        {
            drinkSfx.SetActive(true);
            StartCoroutine(resetDrinkSfx());


            StopCoroutine(throwPotionCooldownCoroutine);
        }
        throwPotionCooldownCoroutine = StartCoroutine(ThrowPotionCoolDown());

    }
    IEnumerator resetDrinkSfx()
    {
        yield return new WaitForSeconds(2);
        drinkSfx.SetActive(false);
    }

    void HealPlayer()
    {
        if (hpBar != null)
        {
            hpBar.currentHP = Mathf.Min(hpBar.currentHP + 20, hpBar.maxHP);
            hpBar.UpdateHPDisplay();
        }
    }

    void HealPlayerDrink()
    {
        if (hpBar != null)
        {
            hpBar.currentHP = Mathf.Min(hpBar.currentHP + 10, hpBar.maxHP);
            hpBar.UpdateHPDisplay();
        }
        healingPlayerFX.SetActive(true);
        Invoke("StopHealingPlayerFX", 2f);
    }

    void StopHealingPlayerFX()
    {
        healingPlayerFX.SetActive(false);
    }

    void ApplyFireEffect()
    {
        if (!currentStatusEffects.Contains("Fire"))
        {
            currentStatusEffects.Add("Fire");
            if (fireStatusEffectCoroutine != null)
            {
                StopCoroutine(fireStatusEffectCoroutine);
            }
            fireStatusEffectCoroutine = StartCoroutine(FireStatusEffect());
        }
    }

    void ApplyElectroEffect()
    {
        if (!currentStatusEffects.Contains("Electro") )
        {
            currentStatusEffects.Add("Electro");
            if (electroDamageCoroutine != null)
            {
                StopCoroutine(electroDamageCoroutine);
            }
            electroDamageCoroutine = StartCoroutine(ElectroStatusEffect());
        }
    }

    void ApplyEarthEffect()
    {
        if (!currentStatusEffects.Contains("Earth"))
        {
            currentStatusEffects.Add("Earth");
            isInvincible = true;
            if (earthStatusEffectCoroutine != null)
            {
                StopCoroutine(earthStatusEffectCoroutine);
            }
            earthStatusEffectCoroutine = StartCoroutine(EarthStatusEffect());
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
        fireStatusEffectCoroutine = null;
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
            TakeDamage(5);
            yield return new WaitForSeconds(1);
            timer -= 1f;
        }

        sprintSpeed = originalSprintSpeed;
        turnSpeed = originalTurnSpeed;
        acceleration = originalAcceleration;

        isSprinting = false;
        electroVFX.SetActive(false);
        currentStatusEffects.Remove("Electro");
        electroStatusIcon.SetActive(false);
        electroDamageCoroutine = null;
    }

    IEnumerator EarthStatusEffect()
    {
        if (earthStatusEffectCoroutine != null)
        {
            StopCoroutine(earthStatusEffectCoroutine);
        }

        earthStatusIcon.SetActive(true);
        earthVFX.SetActive(true);
        isInvincible = true;

        yield return new WaitForSeconds(2);

        isInvincible = false;
        earthVFX.SetActive(false);
        currentStatusEffects.Remove("Earth");
        earthStatusIcon.SetActive(false);
        earthStatusEffectCoroutine = null;

        replacementObject = earthEffect;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible)
        {
            return;
        }

        if (hpBar != null)
        {
            currentStatusEffects.Remove("Earth");
            earthStatusIcon.SetActive(false);
            earthVFX.SetActive(false);
            if (earthStatusEffectCoroutine != null)
            {
                StopCoroutine(earthStatusEffectCoroutine);
                earthStatusEffectCoroutine = null;
            }
            hpBar.DecreaseHP(damage);
            hpBar.UpdateHPDisplay();
        }
    }

    IEnumerator ThrowPotionCoolDown()
    {
        int cooldownDuration = 3;
        if (currentStatusEffects.Contains("Electro"))
        {
            cooldownDuration = 1;
        }
        if (throwPotionTimer != null)
        {
            throwPotionTimer.hours = 0;
            throwPotionTimer.minutes = 0;
            throwPotionTimer.seconds = cooldownDuration;
            throwPotionTimer.StartTimer();
        }

        Debug.Log("Potion cooldown started");

        yield return new WaitForSeconds(cooldownDuration);

        throwPotionCooldown = false;
        Debug.Log("Potion cooldown ended");
    }

    void OnThrowPotionCooldownEnd()
    {
        throwPotionCooldown = false;
        Debug.Log("Potion cooldown ended via timer");
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
