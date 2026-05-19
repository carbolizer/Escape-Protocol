using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth = 3;
    private bool isInvincible = false;
    public float invulnerabilityDuration = 0.9f;
    public float damageFlashDuration = 0.55f;

    private float damageFlashTimer;
    private PlayerController playerController;
    public bool IsDamageFlashing => damageFlashTimer > 0f;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void TakeDamage(int damageAmount)
    {
        if (isInvincible) return;
        if (playerController != null && (playerController.IsExecuting || playerController.IsDashInvincible)) return;

        currentHealth -= damageAmount;
        damageFlashTimer = damageFlashDuration;
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
        yield return new WaitForSeconds(invulnerabilityDuration);
        isInvincible = false;
    }

    private void Update()
    {
        if (damageFlashTimer > 0f)
            damageFlashTimer -= Time.deltaTime;
    }

    private void Die()
    {
        Debug.Log("Player Died!");
        // Reset persistent energy reservoir upon player failure
        if (GameManager.Instance != null) GameManager.Instance.ResetProgress();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}