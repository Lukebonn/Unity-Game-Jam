using UnityEngine;

// The red warning on the ground before an attack lands (inspired by Hades): a strip for dashes and
// bullets, a circle or wedge for swings. Red in the middle, orange at the edges, see-through.
// If you're standing in it when the attack comes, you get hit. It marks where the hit WILL be (it
// doesn't follow the enemy), and removes itself as soon as its owner (enemy or bullet) is gone.
// Made from Resources/GroundTelegraph with GroundTelegraph.Create().
[RequireComponent(typeof(MeshRenderer))]
public class GroundTelegraph : MonoBehaviour
{
    private static readonly int ShapeId = Shader.PropertyToID("_Shape");
    private static readonly int ArcId = Shader.PropertyToID("_Arc");
    private static readonly int FillId = Shader.PropertyToID("_Fill");
    private const float Lift = 0.04f;   // hovers just above the floor
    private static GroundTelegraph prefab;

    private MeshRenderer meshRenderer;
    private MaterialPropertyBlock block;
    private Object owner;

    public static GroundTelegraph Create()
    {
        if (prefab == null) prefab = Resources.Load<GroundTelegraph>("GroundTelegraph");
        return prefab != null ? Instantiate(prefab) : null;
    }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        block = new MaterialPropertyBlock();
    }

    // A strip from start, along direction, length long and width wide
    public void ShowStrip(Object whoseAttack, Vector3 start, Vector3 direction, float length, float width)
    {
        owner = whoseAttack;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.0001f) direction = Vector3.forward;
        direction.Normalize();
        transform.position = OnFloor(start) + direction * (length / 2f);
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(90f, 0f, 0f);
        transform.localScale = new Vector3(width, length, 1f);
        SetShape(0f, Mathf.PI, 0f);
    }

    // A circle (360) or wedge (less) around centre, pointing forward, radius long
    public void ShowArc(Object whoseAttack, Vector3 centre, Vector3 forward, float radius, float degrees)
    {
        owner = whoseAttack;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;
        transform.position = OnFloor(centre);
        transform.rotation = Quaternion.LookRotation(forward.normalized) * Quaternion.Euler(90f, 0f, 0f);
        transform.localScale = new Vector3(radius * 2f, radius * 2f, 1f);
        SetShape(1f, Mathf.Clamp(degrees, 1f, 360f) * 0.5f * Mathf.Deg2Rad, 0f);
    }

    // 0 to 1: how far the charge has got. A brighter band sweeps out to the edge as the hit comes.
    public void SetFill(float fill)
    {
        block.SetFloat(FillId, Mathf.Clamp01(fill));
        meshRenderer.SetPropertyBlock(block);
    }

    private void SetShape(float shape, float halfAngle, float fill)
    {
        block.SetFloat(ShapeId, shape);
        block.SetFloat(ArcId, halfAngle);
        block.SetFloat(FillId, fill);
        meshRenderer.SetPropertyBlock(block);
    }

    private void Update()
    {
        if (owner == null) Destroy(gameObject);
    }

    // Just above the floor under this point (over a hole: level with the floor around it)
    private static Vector3 OnFloor(Vector3 point)
    {
        int floorMask = ~LayerMask.GetMask("Enemy", "Player");
        float y = 0f;
        if (Physics.Raycast(point + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 6f, floorMask, QueryTriggerInteraction.Ignore))
            y = hit.point.y;
        return new Vector3(point.x, y + Lift, point.z);
    }
}
