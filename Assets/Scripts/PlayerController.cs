using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private InputActions inputActions;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private PlayerHealth playerHealth;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private bool isExecuting;
    private Collider2D bodyCollider;
    private bool phasingThroughEnemies;
    private readonly HashSet<Collider2D> phasedEnemyColliders = new HashSet<Collider2D>();
    private Vector2 lastFacing = Vector2.right;
    [SerializeField] private Animator _animator;

    [Header("Movement")]
    public float moveSpeed = 6.5f;
    public float acceleration = 42f;
    public float deceleration = 55f;

    [Header("State")]
    public bool hasBadge = false;
    public bool isHidden = false;
    public bool canHide = false;
    public bool isMoving = false;

    [Header("Dash (Space)")]
    [Tooltip("Burst speed during the dash")]
    public float dashSpeed = 18f;
    [Tooltip("Total dash duration in seconds")]
    public float dashDuration = 0.18f;
    [Tooltip("Cooldown between dashes")]
    public float dashCooldown = 0.7f;
    [Tooltip("Stealth energy refunded for a clean dash, encouraging mobility")]
    public float dashStealthRefund = 18f;
    private float dashTimer;
    private float dashCooldownTimer;
    private Vector2 dashDirection;
    public bool IsDashing => dashTimer > 0f;
    public bool IsDashInvincible => IsDashing;

    [Header("Freeze Potions (Q)")]
    [FormerlySerializedAs("rockPrefab")]
    public GameObject freezePotionPrefab;
    [FormerlySerializedAs("rocksPerLevel")]
    public int freezePotionsPerLevel = 2;
    public float throwSpeed = 14f;
    public float throwLifetime = 1.5f;
    public float distractionRadius = 4.5f;
    public float distractionDuration = 5f;
    private int freezePotionsRemaining;
    public int FreezePotionsRemaining => freezePotionsRemaining;

    [Header("Stealth Kill")]
    public bool canExecute = false;
    public GameObject executionTarget;
    public float executionLungeSpeed = 14f;
    public float executionLungeTime = 0.12f;
    public bool IsExecuting => isExecuting;

    private void Awake()
    {
        inputActions = new InputActions();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerHealth = GetComponent<PlayerHealth>();
        bodyCollider = GetComponent<Collider2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearDamping = 0f;
        freezePotionsRemaining = Mathf.Max(0, freezePotionsPerLevel);
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Standard.Enable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        ClearEnemyPhasing();
        GameManager.ApplyGameplayTimeScale(false);
        inputActions.Standard.Disable();
        inputActions.Disable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        freezePotionsRemaining = Mathf.Max(0, freezePotionsPerLevel);
    }

    private void Update()
    {
        if (GameManager.IsGamePaused)
        {
            moveInput = Vector2.zero;
            return;
        }

        if (isExecuting)
        {
            moveInput = Vector2.zero;
            GameManager.ApplyGameplayTimeScale(false);
            return;
        }

        if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;
        if (dashTimer > 0f)
            dashTimer -= Time.deltaTime;

        ReadMovementInput();
        isMoving = moveInput.sqrMagnitude > 0.01f;

        UpdateSpriteFacing();
        UpdateStealthVisual();
        HandleCombatInput();
        HandleThrowInput();
    }

    private void FixedUpdate()
    {
        if (GameManager.IsGamePaused || isExecuting)
        {
            rb.linearVelocity = Vector2.zero;
            currentVelocity = Vector2.zero;
            return;
        }

        if (IsDashing)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            currentVelocity = rb.linearVelocity;
            return;
        }

        Vector2 targetVelocity = moveInput * moveSpeed;
        float rate = moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;
        currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, rate * Time.fixedDeltaTime);
        rb.linearVelocity = currentVelocity;
    }

    private void ReadMovementInput()
    {
        moveInput = Vector2.zero;

        if (inputActions.Standard.MoveUp.IsPressed()) {
            _animator.SetBool("isRunning", true);
            moveInput = Vector2.up;
        }
        else if (inputActions.Standard.MoveDown.IsPressed()) {
            _animator.SetBool("isRunning", true);
            moveInput = Vector2.down;
        }
        else if (inputActions.Standard.MoveLeft.IsPressed()) {
            _animator.SetBool("isRunning", true);
            moveInput = Vector2.left;
        }
        else if (inputActions.Standard.MoveRight.IsPressed()) {
            _animator.SetBool("isRunning", true);
            moveInput = Vector2.right;
        }
        else {
            _animator.SetBool("isRunning", false);
        }

        if (moveInput.sqrMagnitude > 0.01f)
            lastFacing = moveInput.normalized;
    }

    private void UpdateSpriteFacing()
    {
        if (spriteRenderer == null) return;

        if (moveInput.x < -0.01f)
            spriteRenderer.flipX = true;
        else if (moveInput.x > 0.01f)
            spriteRenderer.flipX = false;
    }

    private void UpdateStealthVisual()
    {
        if (spriteRenderer == null) return;

        bool usingCamo = Keyboard.current.fKey.isPressed &&
                         GameManager.Instance != null &&
                         GameManager.Instance.currentInvisEnergy > 0;

        Color targetColor;
        if (usingCamo)
        {
            isHidden = true;
            GameManager.Instance.currentInvisEnergy -= GameManager.Instance.energyDrainRate * Time.deltaTime;
            if (GameManager.Instance.currentInvisEnergy < 0)
                GameManager.Instance.currentInvisEnergy = 0;

            targetColor = new Color(0.75f, 0.85f, 1f, 0.45f);
            GameManager.ApplyGameplayTimeScale(true);
        }
        else
        {
            isHidden = false;
            GameManager.ApplyGameplayTimeScale(false);
            if (canExecute)
                targetColor = new Color(1f, 0.55f, 0.6f, 1f);
            else
                targetColor = Color.white;
        }

        // Damage feedback should override stealth tint enough to read instantly.
        if (playerHealth != null && playerHealth.IsDamageFlashing)
            targetColor = Color.Lerp(targetColor, Color.red, 0.8f);

        spriteRenderer.color = targetColor;

        UpdateEnemyPhasing();
    }

    private void UpdateEnemyPhasing()
    {
        bool shouldPhase = (isHidden || IsDashing) && !isExecuting;

        if (!shouldPhase)
        {
            if (phasingThroughEnemies)
                ClearEnemyPhasing();
            return;
        }

        phasingThroughEnemies = true;
        if (bodyCollider == null) return;

        foreach (KillableEnemy enemy in FindObjectsByType<KillableEnemy>(FindObjectsSortMode.None))
        {
            foreach (Collider2D enemyCol in enemy.GetComponentsInChildren<Collider2D>())
            {
                if (enemyCol.isTrigger || !phasedEnemyColliders.Add(enemyCol))
                    continue;

                Physics2D.IgnoreCollision(bodyCollider, enemyCol, true);
            }
        }
    }

    private void ClearEnemyPhasing()
    {
        if (bodyCollider != null)
        {
            foreach (Collider2D enemyCol in phasedEnemyColliders)
            {
                if (enemyCol != null)
                    Physics2D.IgnoreCollision(bodyCollider, enemyCol, false);
            }
        }

        phasedEnemyColliders.Clear();
        phasingThroughEnemies = false;
    }

    private void HandleCombatInput()
    {
        if (canExecute && executionTarget != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            
            EnemyExecution execution = executionTarget.GetComponent<EnemyExecution>();
            if (execution != null)
                StartCoroutine(PerformStealthKill(execution));
            
            return;
        }

        if (canExecute) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && dashCooldownTimer <= 0f && !IsDashing)
            StartDash();
    }

    private void StartDash()
    {
        Vector2 dir = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : lastFacing;
        if (dir.sqrMagnitude < 0.0001f)
            dir = spriteRenderer != null && spriteRenderer.flipX ? Vector2.left : Vector2.right;

        dashDirection = dir;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        if (GameManager.Instance != null)
            GameManager.Instance.AddInvisEnergy(dashStealthRefund);
    }

    private void HandleThrowInput()
    {
        if (!Keyboard.current.qKey.wasPressedThisFrame) return;
        if (freezePotionsRemaining <= 0) return;

        Vector2 dir = lastFacing.sqrMagnitude > 0.0001f ? lastFacing.normalized : Vector2.right;
        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.5f);

        GameObject potion = freezePotionPrefab != null
            ? Instantiate(freezePotionPrefab, spawnPos, Quaternion.identity)
            : new GameObject("FreezePotion");

        if (freezePotionPrefab == null)
            potion.transform.position = spawnPos;

        FreezePotionProjectile projectile = potion.GetComponent<FreezePotionProjectile>();
        if (projectile == null)
            projectile = potion.AddComponent<FreezePotionProjectile>();

        projectile.Launch(dir, throwSpeed, throwLifetime, distractionRadius, distractionDuration, transform);
        freezePotionsRemaining--;
    }

    private IEnumerator PerformStealthKill(EnemyExecution execution)
    {
        isExecuting = true;
        canExecute = false;
        GameManager.ApplyGameplayTimeScale(false);
        moveInput = Vector2.zero;
        currentVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        Vector2 start = transform.position;
        Vector2 target = execution.transform.position;
        float elapsed = 0f;

        while (elapsed < executionLungeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / executionLungeTime;
            rb.MovePosition(Vector2.Lerp(start, target, t));
            yield return null;
        }

        execution.StartExecution(this);
        isExecuting = false;
    }
}
