using UnityEngine;
using UnityEngine.UI;

public class InvisUI : MonoBehaviour
{
    [Tooltip("Assign the filled Invisibility UI Image element here")]
    public Image invisBarFill;

    void Update()
    {
        if (GameManager.Instance != null && invisBarFill != null)
        {
            // Normalize current energy to a 0.0 to 1.0 range for the fill slider
            invisBarFill.fillAmount = GameManager.Instance.currentInvisEnergy / GameManager.Instance.maxInvisEnergy;
        }
    }
}