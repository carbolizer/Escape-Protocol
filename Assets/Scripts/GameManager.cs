using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Stealth Resource")]
    public float maxInvisEnergy = 100f;
    public float currentInvisEnergy = 0f; // Starts empty so the player must hunt for chests
    public float energyDrainRate = 25f;   // Drains 25 units per second while active

    private void Awake()
    {
        // Enforce Singleton Pattern to persist across all scene loads
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

    public void AddInvisEnergy(float amount)
    {
        currentInvisEnergy = Mathf.Clamp(currentInvisEnergy + amount, 0, maxInvisEnergy);
    }

    public void ResetProgress()
    {
        currentInvisEnergy = 0f;
    }
}