using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour
{
    public float energyGranted = 40f; // Fills 40% of the maximum resource bar
    private bool isPlayerInRange = false;
    private bool isOpen = false;

    [Tooltip("Optional: Drop an opened chest sprite here to change visuals upon interaction")]
    public Sprite openChestSprite;

    void Update()
    {
        if (GameManager.IsGamePaused) return;

        // Permit looting if close, unlooted, and pressing 'E'
        if (isPlayerInRange && !isOpen && Keyboard.current.eKey.wasPressedThisFrame)
        {
            LootChest();
        }
    }

    void LootChest()
    {
        isOpen = true;
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddInvisEnergy(energyGranted);
            Debug.Log("Looted Chest! Invisibility energy restored.");
        }

        // Apply looted state feedback
        if (openChestSprite != null)
        {
            GetComponent<SpriteRenderer>().sprite = openChestSprite;
        }
        else
        {
            GetComponent<SpriteRenderer>().color = Color.gray; // Fallback visual modification
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) isPlayerInRange = false;
    }
}