using Unity.VisualScripting;
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

    [Header("Combat")]
    public GameObject meleeHitbox;
    public float attackDuration = 0.2f;
    private float attackTimer = 0f;

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

        // HIDING LOGIC
        // If the player is in a hiding spot AND holding the 'F' key
        if (canHide && UnityEngine.InputSystem.Keyboard.current.fKey.isPressed)
        {
            isHidden = true;
            // Make the player semi-transparent to prove they are hiding
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
        }
        else
        {
            isHidden = false;
            // Return to normal color
            GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }

        // COMBAT LOGIC
        // If we press Spacebar and aren't already attacking
        if (UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame && attackTimer <= 0)
        {
            meleeHitbox.SetActive(true); // Turn on the damage zone
            attackTimer = attackDuration; // Start the cooldown
        }

        // Turn the hitbox back off after a split second
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
            {
                meleeHitbox.SetActive(false);
            }
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed * Time.deltaTime;
    }



}
