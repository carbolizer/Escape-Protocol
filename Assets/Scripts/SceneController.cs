using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    [Header("Scene to Load")]
    [Tooltip("Type the exact name of the scene you want to load next")]
    public string nextSceneName;

    public void LoadNextScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "TitleScreen" && nextSceneName == "Level1" && GameManager.Instance != null)
        {
            GameManager.Instance.StartNewRun();
        }

        if (nextSceneName == "WinScreen" && GameManager.Instance != null)
        {
            GameManager.Instance.FinalizeRun();
            GameManager.Instance.ResetProgress();
        }

        if (nextSceneName == "TitleScreen" && GameManager.Instance != null)
        {
            GameManager.Instance.ResetRun();
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
