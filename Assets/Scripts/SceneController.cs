using UnityEngine;
using UnityEngine.SceneManagement; // Required for loading scenes

public class SceneController : MonoBehaviour
{
    [Header("Scene to Load")]
    [Tooltip("Type the exact name of the scene you want to load next")]
    public string nextSceneName;

    // This method will be used by our UI Buttons
    public void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }

    // This method handles the player walking into the exit door
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            // Check if the player exists AND has the badge
            if (player != null && player.hasBadge == true)
            {
                Debug.Log("Access Granted! Loading next level...");
                player.hasBadge = false; // Reset the badge so they have to find a new one in the next level
                LoadNextScene();
            }
            else
            {
                Debug.Log("Door Locked: You need the Security Badge!");
            }
        }
    }
}