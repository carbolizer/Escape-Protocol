using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public AudioSource audioSource;

    public AudioClip footstepClip;
    public AudioClip attackClip;

    public float footstepDelay = 0.35f;
    private float footstepTimer;

    void Update()
    {
        bool isMoving =
            Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0;

        if (isMoving)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f && !audioSource.isPlaying)
            {
                audioSource.PlayOneShot(footstepClip);
                footstepTimer = footstepDelay;
            }
        }
        else
        {
            footstepTimer = 0f;
            audioSource.Stop();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            audioSource.PlayOneShot(attackClip);
        }
    }
}
