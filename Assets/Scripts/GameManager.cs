using HighScore;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool IsGamePaused { get; private set; }

    [Header("Stealth Resource")]
    public float maxInvisEnergy = 100f;
    public float currentInvisEnergy = 0f;
    public float energyDrainRate = 25f;

    [Header("Scoring")]
    [Tooltip("Must stay identical every launch — used by the online high score server")]
    public string highScoreGameName = "Escape Protocol 2";
    public int defaultEnemyPointValue = 100;
    public int maxTimeBonus = 5000;
    public int timePenaltyPerSecond = 10;

    [Header("Run Stats (read-only during play)")]
    public int EnemiesKilled { get; private set; }
    public int KillScore { get; private set; }
    public int TimeBonus { get; private set; }
    public int FinalScore { get; private set; }
    public float RunElapsedSeconds { get; private set; }
    public bool IsRunActive { get; private set; }
    public bool RunFinalized { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        HS.Init(this, highScoreGameName);
    }

    private void Update()
    {
        if (!IsRunActive || RunFinalized) return;
        RunElapsedSeconds += Time.deltaTime;
    }

    public void StartNewRun()
    {
        EnemiesKilled = 0;
        KillScore = 0;
        TimeBonus = 0;
        FinalScore = 0;
        RunElapsedSeconds = 0f;
        IsRunActive = true;
        RunFinalized = false;
        ResetProgress();
    }

    public void RegisterEnemyKill(int points)
    {
        if (!IsRunActive || RunFinalized) return;

        EnemiesKilled++;
        KillScore += Mathf.Max(0, points);
    }

    public void FinalizeRun()
    {
        if (RunFinalized) return;

        IsRunActive = false;
        RunFinalized = true;

        int timePenalty = Mathf.FloorToInt(RunElapsedSeconds) * timePenaltyPerSecond;
        TimeBonus = Mathf.Max(0, maxTimeBonus - timePenalty);
        FinalScore = KillScore + TimeBonus;
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

        return KillScore + GetProjectedTimeBonus();
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
        KillScore = 0;
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
        Time.timeScale = paused ? 0f : 1f;
    }
}
