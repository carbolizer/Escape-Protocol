using UnityEngine;
using UnityEngine.SceneManagement; 

public class EnemyVision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Did the player step into the vision cone
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            // If the player exists and is NOT hiding, they get caught
            if (player != null && player.isHidden == false)
            {
                Debug.Log("Spotted! Restarting level...");

                // This instantly reloads the current active scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}