using UnityEngine;
using System.Collections; // Required for Coroutines

public class EnemyExecution : MonoBehaviour
{
    public float executionTime = 0.5f; // How long the enemy flashes red
    private bool isBeingExecuted = false;

    public void StartExecution()
    {
        // Prevent triggering the execution multiple times
        if (isBeingExecuted) return;

        isBeingExecuted = true;
        StartCoroutine(ExecuteRoutine());
    }

    private IEnumerator ExecuteRoutine()
    {
        // 1. Flash Red
        GetComponent<SpriteRenderer>().color = Color.red;

        // 2. Paralyze the enemy so they can't catch you while they die
        GetComponent<Collider2D>().enabled = false;
        if (GetComponent<EnemyVision>() != null) GetComponent<EnemyVision>().enabled = false;
        if (GetComponent<ConeSweep>() != null) GetComponent<ConeSweep>().enabled = false;

        // 3. Wait for the animation to finish
        yield return new WaitForSeconds(executionTime);

        // 4. Disappear!
        Destroy(gameObject);
    }
}