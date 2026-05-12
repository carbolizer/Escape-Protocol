using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class SceneController : MonoBehaviour
{
    [Header("Level Exit")]
    [Tooltip("Type the exact name of the scene you want to load next")]
    public string nextSceneName;

    [Header("Pause Menu UI")]
    public GameObject pauseMenuPanel;
    public GameObject controlsPanel;
    private bool isPaused = false;

    void Update()
    {
        // Toggle Pause when Escape is hit
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
            {
                // If the controls panel is open, hitting escape just goes back to the pause menu
                if (controlsPanel != null && controlsPanel.activeSelf)
                {
                    CloseControls();
                }
                else
                {
                    ResumeGame();
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Freezes game physics and standard timers

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Resumes normal game speed

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    public void OpenControls()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (controlsPanel != null) controlsPanel.SetActive(true);
    }

    public void CloseControls()
    {
        if (controlsPanel != null) controlsPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
    }

    public void LoadNextScene()
    {
        // Ensure time scale is reset to 1 before loading a new scene!
        Time.timeScale = 1f;

        if (SceneManager.GetActiveScene().name == "TitleScreen" && GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }

        if (nextSceneName == "WinScreen" && GameManager.Instance != null)
        {
            GameManager.Instance.ResetProgress();
        }

        SceneManager.LoadScene(nextSceneName);
    }

    public void QuitToTitle()
    {
        Time.timeScale = 1f; // Always reset time scale before leaving!
        if (GameManager.Instance != null) GameManager.Instance.ResetProgress();
        SceneManager.LoadScene("TitleScreen");
    }

    public void QuitGame()
    {
        Debug.Log("Game is Exiting!");
        Application.Quit();
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
}