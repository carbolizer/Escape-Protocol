using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class LaserDoor : MonoBehaviour
{
    public float beamLength = 3f;
    public float beamThickness = 0.18f;
    public bool horizontal = true;
    public float toggleInterval = 1.5f;
    public float startOffset;
    public int damage = 1;

    private BoxCollider2D beamCollider;
    private SpriteRenderer beamRenderer;
    private bool isActive = true;
    private static Sprite beamSprite;

    private void Awake()
    {
        beamCollider = GetComponent<BoxCollider2D>();
        beamCollider.isTrigger = true;

        EnsureVisual();
        ApplyBeamSize();
    }

    private void Start()
    {
        StartCoroutine(ToggleRoutine());
    }

    private IEnumerator ToggleRoutine()
    {
        if (startOffset > 0f)
            yield return new WaitForSeconds(startOffset);

        while (true)
        {
            SetActive(!isActive);
            yield return new WaitForSeconds(toggleInterval);
        }
    }

    private void SetActive(bool active)
    {
        isActive = active;
        beamCollider.enabled = active;

        if (beamRenderer != null)
        {
            beamRenderer.enabled = true;
            beamRenderer.color = active
                ? new Color(1f, 0.15f, 0.25f, 0.92f)
                : new Color(0.35f, 0.1f, 0.15f, 0.25f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive || GameManager.IsGamePaused) return;
        if (!collision.CompareTag("Player")) return;

        PlayerController player = collision.GetComponentInParent<PlayerController>();
        if (player != null && player.isHidden) return;

        PlayerHealth health = collision.GetComponentInParent<PlayerHealth>();
        if (health != null)
            health.TakeDamage(damage);
    }

    private void EnsureVisual()
    {
        if (beamSprite == null)
            beamSprite = CreateBeamSprite();

        Transform existing = transform.Find("BeamVisual");
        if (existing != null)
        {
            beamRenderer = existing.GetComponent<SpriteRenderer>();
            return;
        }

        GameObject visual = new GameObject("BeamVisual");
        visual.transform.SetParent(transform, false);
        beamRenderer = visual.AddComponent<SpriteRenderer>();
        beamRenderer.sprite = beamSprite;
        beamRenderer.sortingLayerName = "Default";
        beamRenderer.sortingOrder = 15;
    }

    private void ApplyBeamSize()
    {
        Vector2 size = horizontal
            ? new Vector2(beamLength, beamThickness)
            : new Vector2(beamThickness, beamLength);

        beamCollider.size = size;
        beamCollider.offset = Vector2.zero;

        if (beamRenderer != null)
        {
            beamRenderer.transform.localScale = new Vector3(size.x, size.y, 1f);
        }
    }

    private static Sprite CreateBeamSprite()
    {
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        Color32[] pixels = new Color32[16];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, 255);
        tex.SetPixels32(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }
}
