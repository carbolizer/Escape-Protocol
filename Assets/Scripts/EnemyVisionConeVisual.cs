using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class EnemyVisionConeVisual : MonoBehaviour
{
    [Tooltip("Number of rays used to build the cone outline (more = smoother)")]
    public int rayCount = 32;
    public Color patrolColor = new Color(1f, 0.85f, 0.2f, 0.18f);
    public Color aggroColor = new Color(1f, 0.2f, 0.2f, 0.32f);
    public string sortingLayer = "Decor";
    public int sortingOrder = -10;

    private EnemyAI owner;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;
    private int[] triangles;
    private static Material sharedMaterial;

    public static EnemyVisionConeVisual AttachTo(EnemyAI ai)
    {
        if (ai == null) return null;

        GameObject go = new GameObject("EnemyVisionCone");
        go.transform.SetParent(ai.transform, false);

        EnemyVisionConeVisual cone = go.AddComponent<EnemyVisionConeVisual>();
        cone.Initialize(ai);
        return cone;
    }

    private void Initialize(EnemyAI ai)
    {
        owner = ai;
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh { name = "EnemyVisionConeMesh" };
        mesh.MarkDynamic();
        meshFilter.sharedMesh = mesh;

        if (sharedMaterial == null)
        {
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null)
                shader = Shader.Find("Unlit/Transparent");
            sharedMaterial = new Material(shader != null ? shader : Shader.Find("Hidden/InternalErrorShader"));
        }

        meshRenderer.sharedMaterial = sharedMaterial;
        meshRenderer.sortingLayerName = sortingLayer;
        meshRenderer.sortingOrder = sortingOrder;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    private void LateUpdate()
    {
        if (owner == null)
        {
            meshRenderer.enabled = false;
            return;
        }

        BuildMesh();
    }

    private void BuildMesh()
    {
        int count = Mathf.Max(8, rayCount);
        int vertCount = count + 2;
        if (vertices == null || vertices.Length != vertCount)
        {
            vertices = new Vector3[vertCount];
            colors = new Color[vertCount];
            triangles = new int[count * 3];
            for (int i = 0; i < count; i++)
            {
                triangles[i * 3 + 0] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        Vector2 origin = owner.transform.position;
        Vector2 forward = owner.VisionForward.right;
        if (forward.sqrMagnitude < 0.0001f)
            forward = owner.transform.right;
        forward.Normalize();

        float halfAngle = owner.VisionHalfAngle;
        float range = owner.EffectiveVisionRange;
        LayerMask blockers = owner.SightBlockerMask;
        Color tint = owner.IsAggro ? aggroColor : patrolColor;

        vertices[0] = transform.InverseTransformPoint(origin);
        colors[0] = tint;

        for (int i = 0; i <= count; i++)
        {
            float t = (float)i / count;
            float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
            Vector2 dir = Rotate(forward, angle);
            float hitDistance = CastRay(origin, dir, range, blockers);

            Vector2 worldPoint = origin + dir * hitDistance;
            vertices[i + 1] = transform.InverseTransformPoint(worldPoint);
            colors[i + 1] = tint;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
    }

    private float CastRay(Vector2 origin, Vector2 direction, float maxDistance, LayerMask mask)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, direction, maxDistance, mask);
        float bestDistance = maxDistance;

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D col = hits[i].collider;
            if (col == null || col.isTrigger) continue;
            if (col.transform == owner.transform || col.transform.IsChildOf(owner.transform)) continue;
            if (col.GetComponentInParent<PlayerController>() != null) continue;

            if (hits[i].distance < bestDistance)
                bestDistance = hits[i].distance;
        }

        return bestDistance;
    }

    private static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad);
        float s = Mathf.Sin(rad);
        return new Vector2(c * v.x - s * v.y, s * v.x + c * v.y);
    }
}
