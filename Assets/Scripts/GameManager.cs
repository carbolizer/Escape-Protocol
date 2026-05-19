using System.Collections;
using HighScore;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool IsGamePaused { get; private set; }

    [Header("Stealth Resource")]
    public float maxInvisEnergy = 100f;
    public float currentInvisEnergy = 0f;
    public float energyDrainRate = 25f;
    [Tooltip("Time scale while holding camouflage (F)")]
    [Range(0.1f, 1f)]
    public float camoTimeScale = 0.4f;

    [Header("Scoring")]
    [Tooltip("Must stay identical every launch — used by the online high score server")]
    public string highScoreGameName = "Escape Protocol 2";
    public int defaultEnemyPointValue = 60;
    public int stealthKillPointValue = 150;
    public int maxTimeBonus = 5000;
    public int timePenaltyPerSecond = 10;

    [Header("Run Stats (read-only during play)")]
    public int EnemiesKilled { get; private set; }
    public int StealthKills { get; private set; }
    public int KillScore { get; private set; }
    public int CollectibleScore { get; private set; }
    public int CollectiblesGathered { get; private set; }
    public int TimeBonus { get; private set; }
    public int FinalScore { get; private set; }
    public float RunElapsedSeconds { get; private set; }
    public bool IsRunActive { get; private set; }
    public bool RunFinalized { get; private set; }

    [Header("HUD")]
    [SerializeField] private GameObject stealthBarPrefab;

    private static readonly string[] GameplayScenes =
        { "Level1", "Level2", "Level3", "BeginningDungeon" };

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureCameraShake();
        AssassinatePromptHUD.EnsureExists();
        RockCountHUD.EnsureExists();

        if (Instance != null)
            Instance.EnsureStealthBar(scene);
    }

    private void Start()
    {
        HS.Init(this, highScoreGameName);
        EnsureCameraShake();
    }

    private static void EnsureCameraShake()
    {
        if (CameraShake.Instance != null) return;

        Camera main = Camera.main;
        if (main != null && main.GetComponent<CameraShake>() == null)
            main.gameObject.AddComponent<CameraShake>();
    }

    private void EnsureStealthBar(Scene scene)
    {
        if (!IsGameplayScene(scene.name)) return;
        if (FindAnyObjectByType<InvisUI>() != null) return;
        if (stealthBarPrefab == null) return;

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;

        GameObject bar = Instantiate(stealthBarPrefab, canvas.transform);
        bar.name = "InvisiBarBG";
    }

    private static bool IsGameplayScene(string sceneName)
    {
        for (int i = 0; i < GameplayScenes.Length; i++)
        {
            if (GameplayScenes[i] == sceneName)
                return true;
        }

        return false;
    }

    private void Update()
    {
        if (!IsRunActive || RunFinalized) return;
        RunElapsedSeconds += Time.deltaTime;
    }

    public void StartNewRun()
    {
        EnemiesKilled = 0;
        StealthKills = 0;
        KillScore = 0;
        CollectibleScore = 0;
        CollectiblesGathered = 0;
        TimeBonus = 0;
        FinalScore = 0;
        RunElapsedSeconds = 0f;
        IsRunActive = true;
        RunFinalized = false;
        ResetProgress();
    }

    public void RegisterCollectible(int points)
    {
        if (!IsRunActive || RunFinalized) return;

        CollectiblesGathered++;
        CollectibleScore += Mathf.Max(0, points);
    }

    public void RegisterEnemyKill(int points, bool wasStealthKill)
    {
        if (!IsRunActive || RunFinalized) return;

        EnemiesKilled++;
        if (wasStealthKill) StealthKills++;
        KillScore += Mathf.Max(0, points);
    }

    public void RegisterStealthKill(int points)
    {
        RegisterEnemyKill(points, true);
    }

    public void FinalizeRun()
    {
        if (RunFinalized) return;

        IsRunActive = false;
        RunFinalized = true;

        int timePenalty = Mathf.FloorToInt(RunElapsedSeconds) * timePenaltyPerSecond;
        TimeBonus = Mathf.Max(0, maxTimeBonus - timePenalty);
        FinalScore = KillScore + CollectibleScore + TimeBonus;
    }

    public string GetFormattedRunTime()
    {
        int totalSeconds = Mathf.FloorToInt(RunElapsedSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:00}:{seconds:00}";
    }

    public int GetProjectedTimeBonus()
    {
        int timePenalty = Mathf.FloorToInt(RunElapsedSeconds) * timePenaltyPerSecond;
        return Mathf.Max(0, maxTimeBonus - timePenalty);
    }

    public int GetCurrentScore()
    {
        if (RunFinalized)
            return FinalScore;

        return KillScore + CollectibleScore + GetProjectedTimeBonus();
    }

    public void AddInvisEnergy(float amount)
    {
        currentInvisEnergy = Mathf.Clamp(currentInvisEnergy + amount, 0, maxInvisEnergy);
    }

    public void ResetProgress()
    {
        currentInvisEnergy = 0f;
    }

    public void ResetRun()
    {
        SetGamePaused(false);
        EnemiesKilled = 0;
        StealthKills = 0;
        KillScore = 0;
        CollectibleScore = 0;
        CollectiblesGathered = 0;
        TimeBonus = 0;
        FinalScore = 0;
        RunElapsedSeconds = 0f;
        IsRunActive = false;
        RunFinalized = false;
        ResetProgress();
    }

    public static void SetGamePaused(bool paused)
    {
        IsGamePaused = paused;
        if (paused)
            Time.timeScale = 0f;
        else
            RefreshGameplayTimeScale();
    }

    public static void ApplyGameplayTimeScale(bool camoActive)
    {
        if (IsGamePaused || Instance == null) return;
        Time.timeScale = camoActive ? Instance.camoTimeScale : 1f;
    }

    public static void RefreshGameplayTimeScale()
    {
        if (IsGamePaused || Instance == null) return;

        PlayerController player = FindAnyObjectByType<PlayerController>();
        bool camoActive = player != null && player.isHidden;
        ApplyGameplayTimeScale(camoActive);
    }
}
