using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth = 3;
    private bool isInvincible = false;

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible) return;

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
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(1f);
        GetComponent<SpriteRenderer>().color = Color.white;
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        // Reset persistent energy reservoir upon player failure
        if (GameManager.Instance != null) GameManager.Instance.ResetProgress();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}