using UnityEngine;

public class HidingSpot : MonoBehaviour
{
    // When the player steps INTO the hiding spot
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().canHide = true;
        }
    }

    // When the player steps OUT of the hiding spot
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().canHide = false;
        }
    }
}