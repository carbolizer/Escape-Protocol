using UnityEngine;

public class EnemyPoints : MonoBehaviour
{
    [Tooltip("Points awarded when this enemy is killed")]
    public int pointValue = 100;

    private bool killScored;

    public void AwardKillPoints()
    {
        if (killScored) return;
        killScored = true;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyKill(pointValue);
    }
}
