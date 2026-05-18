using UnityEngine;
using System.Collections;

public class EnemyExecution : MonoBehaviour
{
    public float executionTime = 0.5f;
    private bool isBeingExecuted;

    public void StartExecution()
    {
        if (isBeingExecuted) return;

        isBeingExecuted = true;
        StartCoroutine(ExecuteRoutine());
    }

    private IEnumerator ExecuteRoutine()
    {
        GetComponent<SpriteRenderer>().color = Color.red;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;
        if (GetComponent<EnemyVision>() != null) GetComponent<EnemyVision>().enabled = false;
        if (GetComponent<ConeSweep>() != null) GetComponent<ConeSweep>().enabled = false;

        yield return new WaitForSeconds(executionTime);

        EnemyPoints points = GetComponent<EnemyPoints>();
        if (points != null)
            points.AwardKillPoints();
        else if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyKill(GameManager.Instance.defaultEnemyPointValue);

        Destroy(gameObject);
    }
}
