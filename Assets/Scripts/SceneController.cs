using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class SceneController : MonoBehaviour
{
    void Update()
    {
        // If we are in Level 1 and the user hits Escape, load the Title Screen
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // Change "TitleScreen" to whatever your main menu scene is named
            SceneManager.LoadScene("TitleScreen");
        }
    }

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
    public void QuitGame()
    {
        Debug.Log("Game is Exiting!"); 
        Application.Quit(); // This is the line that actually closes the built .exe
    }
}