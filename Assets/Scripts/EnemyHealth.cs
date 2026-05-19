using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 1;
    public bool isHeavy;

    private int currentHealth;
    private SpriteRenderer spriteRenderer;
    private Color baseColor = Color.white;

    private void Awake()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            baseColor = spriteRenderer.color;
    }

    public void ConfigureHeavy(int health, Color tint, float scaleMultiplier)
    {
        isHeavy = true;
        maxHealth = health;
        currentHealth = health;
        baseColor = tint;

        if (spriteRenderer != null)
            spriteRenderer.color = tint;

        transform.localScale *= scaleMultiplier;
    }

    public bool ApplyDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth > 0)
        {
            StartCoroutine(DamageFlash());
            return false;
        }

        return true;
    }

    private IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = Color.white;
        yield return new WaitForSeconds(0.08f);
        spriteRenderer.color = baseColor;
    }
}
