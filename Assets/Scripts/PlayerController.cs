using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputActions inputActions;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    public float speed;
    public bool hasBadge = false;
    public bool isHidden = false;
    public bool canHide = false;
    public bool isMoving = false;

    [Header("Combat")]
    public GameObject meleeHitbox;
    public float attackDuration = 0.2f;
    private float attackTimer = 0f;

    [Header("Stealth Kill")]
    public bool canExecute = false;
    public GameObject executionTarget;

    private void Awake()
    {
        inputActions = new();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Standard.Enable();
    }

    private void OnDisable()
    {
        inputActions.Standard.Disable();
        inputActions.Disable();
    }

    private void Update()
    {
        moveInput = Vector2.zero;

        if (inputActions.Standard.MoveUp.IsPressed())
        {
            moveInput = Vector2.up;
        }
        else if (inputActions.Standard.MoveDown.IsPressed())
        {
            moveInput = Vector2.down;
        }
        else if (inputActions.Standard.MoveLeft.IsPressed())
        {
            moveInput = Vector2.left;
        }
        else if (inputActions.Standard.MoveRight.IsPressed())
        {
            moveInput = Vector2.right;
        }

        isMoving = moveInput != Vector2.zero;

        // --- DYNAMIC CAMOUFLAGE LOGIC (Hold F) ---
        if (UnityEngine.InputSystem.Keyboard.current.fKey.isPressed && GameManager.Instance != null && GameManager.Instance.currentInvisEnergy > 0)
        {
            isHidden = true;

            GameManager.Instance.currentInvisEnergy -= GameManager.Instance.energyDrainRate * Time.deltaTime;
            if (GameManager.Instance.currentInvisEnergy < 0) GameManager.Instance.currentInvisEnergy = 0;

            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.4f);
        }
        else
        {
            isHidden = false;
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }

        // COMBAT LOGIC (Spacebar)
        if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame && attackTimer <= 0)
        {
            meleeHitbox.SetActive(true);
            attackTimer = attackDuration;
        }

        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                meleeHitbox.SetActive(false);
            }
        }

        // --- UPDATED EXECUTION LOGIC (Press E) ---
        // Swapped to 'E' so it doesn't conflict with holding 'F' to hide!
        if (canExecute && UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (executionTarget != null)
            {
                executionTarget.GetComponent<EnemyExecution>().StartExecution();
            }
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed * Time.deltaTime;
    }
}