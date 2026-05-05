using UnityEngine;

public class ConeSweep : MonoBehaviour
{
    public float sweepSpeed = 2f; // How fast they look back and forth
    public float sweepAngle = 45f; // How far they look left and right
    private float startRotation;

    void Start()
    {
        // Remember the direction they are facing at the start
        startRotation = transform.eulerAngles.z;
    }

    void Update()
    {
        // Calculate a smooth back-and-forth rotation using sine waves
        float currentAngle = startRotation + Mathf.Sin(Time.time * sweepSpeed) * sweepAngle;
        transform.rotation = Quaternion.Euler(0, 0, currentAngle);
    }
}