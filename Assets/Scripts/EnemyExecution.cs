using System.Collections;
using UnityEngine;

public class EnemyExecution : MonoBehaviour
{
    public float executionTime = 0.35f;
    private bool isBeingExecuted;

    public void StartExecution(PlayerController attacker)
    {
        if (isBeingExecuted) return;
        isBeingExecuted = true;
        StartCoroutine(ExecuteRoutine(attacker));
    }

    private IEnumerator ExecuteRoutine(PlayerController attacker)
    {
        DisableThreatComponents();

        SpriteRenderer enemySprite = GetComponent<SpriteRenderer>();
        SpriteRenderer playerSprite = attacker != null ? attacker.GetComponent<SpriteRenderer>() : null;

        if (CameraShake.Instance != null)
            CameraShake.Instance.Shake(0.12f, 0.2f);

        float hitStop = 0.06f;
        Time.timeScale = 0.15f;
        yield return new WaitForSecondsRealtime(hitStop);
        GameManager.RefreshGameplayTimeScale();

        for (int i = 0; i < 3; i++)
        {
            if (enemySprite != null)
                enemySprite.color = Color.white;
            if (playerSprite != null)
                playerSprite.color = Color.white;
            yield return new WaitForSeconds(executionTime / 6f);

            if (enemySprite != null)
                enemySprite.color = new Color(1f, 0.2f, 0.25f, 1f);
            yield return new WaitForSeconds(executionTime / 6f);
        }

        AwardStealthKillPoints();
        Destroy(gameObject);
    }

    private void DisableThreatComponents()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        EnemyAI ai = GetComponent<EnemyAI>();
        if (ai != null) ai.enabled = false;

        if (GetComponent<EnemyVision>() != null) GetComponent<EnemyVision>().enabled = false;
        if (GetComponent<ConeSweep>() != null) GetComponent<ConeSweep>().enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void AwardStealthKillPoints()
    {
        EnemyPoints points = GetComponent<EnemyPoints>();
        if (points != null)
            points.AwardStealthKillPoints();
        else if (GameManager.Instance != null)
            GameManager.Instance.RegisterStealthKill(GameManager.Instance.stealthKillPointValue);
    }
}
