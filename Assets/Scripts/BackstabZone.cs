using UnityEngine;

public class BackstabZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.canExecute = true;
                // Tell the player that the parent of this zone (the Enemy) is the target
                player.executionTarget = transform.parent.gameObject;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.canExecute = false;
                player.executionTarget = null;
            }
        }
    }
}