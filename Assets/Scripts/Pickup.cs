using UnityEngine;

public class Pickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object colliding is the player
        if (collision.CompareTag("Player"))
        {
            // Access the PlayerController script on the player
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player != null)
            {
                player.hasBadge = true; // Give the player the badge
                Debug.Log("Badge acquired!");
                Destroy(gameObject); // Remove the item from the scene so it looks like we picked it up
            }
        }
    }
}