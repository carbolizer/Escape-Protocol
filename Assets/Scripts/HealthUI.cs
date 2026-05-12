using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image[] hearts;
    public Sprite fullHeart;
    public Sprite emptyHeart;

    void Start()
    {
        // Automatically find the player's health script in the scene if the slot is empty!
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }
    }

    void Update()
    {
        // Safety check: Do not run the loop if the player was destroyed or hasn't loaded yet
        if (playerHealth == null) return;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < playerHealth.currentHealth)
            {
                hearts[i].sprite = fullHeart;
            }
            else
            {
                hearts[i].sprite = emptyHeart;
            }
        }
    }
}