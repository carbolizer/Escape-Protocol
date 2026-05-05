using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Did the player step into the vision cone?
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            // If the player exists and is NOT hiding, they get caught
            if (player != null && player.isHidden == false)
            {
                Debug.Log("Spotted! Taking damage...");

                // Find the health script and deal 1 damage instead of insta-killing
                PlayerHealth health = collision.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(1);
                }
            }
        }
    }
}