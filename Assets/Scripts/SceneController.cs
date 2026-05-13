using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneController : MonoBehaviour
{
    void Update()
    {
        // Flush global resource if backing out to title screen via Escape
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (GameManager.Instance != null) GameManager.Instance.ResetProgress();
            SceneManager.LoadScene("TitleScreen");
        }
    }

    [Header("Scene to Load")]
    [Tooltip("Type the exact name of the scene you want to load next")]
    public string nextSceneName;

    public void LoadNextScene()
    {
        // Ensure old energy states do not bleed into a brand new game loop
        if (SceneManager.GetActiveScene().name == "TitleScreen" && GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }

        // Flush pool upon reaching final validation milestones
        if (nextSceneName == "WinScreen" && GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();

            if (player != null && player.hasBadge == true)
            {
                Debug.Log("Access Granted! Loading next level...");
                player.hasBadge = false;
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
        Application.Quit();
    }
}