using UnityEngine;

public class EnemyPoints : MonoBehaviour
{
    [Tooltip("Points for a loud weapon kill")]
    public int pointValue = 60;

    [Tooltip("Bonus points for a stealth execution")]
    public int stealthKillValue = 150;

    private bool killScored;

    public void AwardKillPoints()
    {
        if (killScored) return;
        killScored = true;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyKill(pointValue, false);
    }

    public void AwardStealthKillPoints()
    {
        if (killScored) return;
        killScored = true;

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyKill(stealthKillValue, true);
    }
}
