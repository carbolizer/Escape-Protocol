using UnityEngine;

public class KillableEnemy : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Weapon"))
        {
            AwardPointsAndDestroy();
        }
    }

    public void AwardPointsAndDestroy()
    {
        EnemyPoints points = GetComponent<EnemyPoints>();
        if (points != null)
            points.AwardKillPoints();
        else if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemyKill(GameManager.Instance.defaultEnemyPointValue, false);

        Destroy(gameObject);
    }
}
