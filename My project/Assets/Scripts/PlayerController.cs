using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InputActions inputActions;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    public float speed;

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
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = moveInput * speed * Time.deltaTime;
    }

}
