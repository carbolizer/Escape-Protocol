using UnityEngine;

public class KillableEnemy : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the player's weapon hitbox hits this enemy
        if (collision.CompareTag("Weapon"))
        {
            Debug.Log("Enemy Defeated!");
            Destroy(gameObject); // Kill the enemy
        }
    }
}