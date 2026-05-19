using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    private InputActions inputActions;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private bool isExecuting;
    private Collider2D bodyCollider;
    private bool phasingThroughEnemies;
    private readonly HashSet<Collider2D> phasedEnemyColliders = new HashSet<Collider2D>();

    [Header("Movement")]
    public float moveSpeed = 6.5f;
    public float acceleration = 42f;
    public float deceleration = 55f;

    [Header("State")]
    public bool hasBadge = false;
    public bool isHidden = false;
    public bool canHide = false;
    public bool isMoving = false;

    [Header("Combat")]
    public GameObject meleeHitbox;
    public float attackDuration = 0.2f;
    private float attackTimer;

    [Header("Stealth Kill")]
    public bool canExecute = false;
    public GameObject executionTarget;
    public float executionLungeSpeed = 14f;
    public float executionLungeTime = 0.12f;

    private void Awake()
    {
        inputActions = new InputActions();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bodyCollider = GetComponent<Collider2D>();
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.linearDamping = 0f;
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Standard.Enable();
    }

    private void OnDisable()
    {
        ClearEnemyPhasing();
        GameManager.ApplyGameplayTimeScale(false);
        inputActions.Standard.Disable();
        inputActions.Disable();
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

        ReadMovementInput();
        isMoving = moveInput.sqrMagnitude > 0.01f;

        UpdateSpriteFacing();
        UpdateStealthVisual();
        HandleCombatInput();
    }

    private void FixedUpdate()
    {
        if (GameManager.IsGamePaused || isExecuting)
        {
            rb.linearVelocity = Vector2.zero;
            currentVelocity = Vector2.zero;
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

        if (inputActions.Standard.MoveUp.IsPressed()) moveInput = Vector2.up;
        else if (inputActions.Standard.MoveDown.IsPressed()) moveInput = Vector2.down;
        else if (inputActions.Standard.MoveLeft.IsPressed()) moveInput = Vector2.left;
        else if (inputActions.Standard.MoveRight.IsPressed()) moveInput = Vector2.right;
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

        if (usingCamo)
        {
            isHidden = true;
            GameManager.Instance.currentInvisEnergy -= GameManager.Instance.energyDrainRate * Time.deltaTime;
            if (GameManager.Instance.currentInvisEnergy < 0)
                GameManager.Instance.currentInvisEnergy = 0;

            spriteRenderer.color = new Color(0.75f, 0.85f, 1f, 0.45f);
            GameManager.ApplyGameplayTimeScale(true);
        }
        else
        {
            isHidden = false;
            GameManager.ApplyGameplayTimeScale(false);
            if (canExecute)
                spriteRenderer.color = new Color(1f, 0.55f, 0.6f, 1f);
            else
                spriteRenderer.color = Color.white;
        }

        UpdateEnemyPhasing();
    }

    private void UpdateEnemyPhasing()
    {
        bool shouldPhase = isHidden && !isExecuting;

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

        if (Keyboard.current.spaceKey.wasPressedThisFrame && attackTimer <= 0f)
        {
            meleeHitbox.SetActive(true);
            attackTimer = attackDuration;
        }

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f)
                meleeHitbox.SetActive(false);
        }
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
