using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth = 3;
    private bool isInvincible = false;

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible) return; // Ignore damage if we just got hit

        currentHealth -= damageAmount;
        Debug.Log("Player Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityCooldown());
        }
    }

    private IEnumerator InvulnerabilityCooldown()
    {
        isInvincible = true;

        // Optional: Make the sprite flash red here if you want!
        GetComponent<SpriteRenderer>().color = Color.red;

        yield return new WaitForSeconds(1f); // 1 second of safety

        GetComponent<SpriteRenderer>().color = Color.white;
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        // Reload the current level when the player dies
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}