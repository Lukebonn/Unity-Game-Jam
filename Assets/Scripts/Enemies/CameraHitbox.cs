using UnityEngine;

// The box the player's bullets hit, added to every enemy by EnemyBase. Bullets fly flat at about
// chest height, but the camera looks down at an angle, so an enemy's body covers the screen from
// its feet to the top of its head. This box stretches toward and away from the camera to cover
// exactly what you see, so a shot that looks like a hit is a hit. It re-measures the visible body
// every half second, so it keeps matching the model even when the model or animation changes.
// It's a trigger, so it never pushes anything around.
[RequireComponent(typeof(BoxCollider))]
public class CameraHitbox : MonoBehaviour
{
    [SerializeField] private float bulletHeight = 1.3f;   // how high the player's bullets fly above the floor
    [SerializeField] private float extraTop = 1.5f;       // reaches past the head, so nothing flies over

    private BoxCollider box;
    private Renderer[] parts;
    private Quaternion facing = Quaternion.identity;
    private float cot = 1f;            // how much ground depth one metre of height covers on screen
    private float floorBelowRoot = 1f; // the enemy's root sits this far above the floor
    private float nextRefit;

    private void Start()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;

        Transform enemy = transform.parent;
        var found = new System.Collections.Generic.List<Renderer>();
        foreach (Renderer r in enemy.GetComponentsInChildren<Renderer>(true))
            if (r is MeshRenderer || r is SkinnedMeshRenderer) found.Add(r);
        parts = found.ToArray();

        CharacterController controller = enemy.GetComponent<CharacterController>();
        if (controller != null) floorBelowRoot = controller.height / 2f - controller.center.y;

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 angles = cam.transform.eulerAngles;
            facing = Quaternion.Euler(0f, angles.y, 0f);
            cot = 1f / Mathf.Tan(Mathf.Clamp(angles.x, 10f, 89f) * Mathf.Deg2Rad);
        }
        Refit();
    }

    private void LateUpdate()
    {
        // Always lined up with the camera, whichever way the enemy turns
        transform.rotation = facing;

        if (Time.time >= nextRefit)
            Refit();
    }

    private void Refit()
    {
        nextRefit = Time.time + 0.5f;

        // Where the body is right now (it can sit off the enemy's centre or float above the floor)
        bool any = false;
        Bounds body = new Bounds(transform.position, Vector3.one);
        foreach (Renderer r in parts)
        {
            if (r == null || !r.enabled || !r.gameObject.activeInHierarchy) continue;
            if (!any) { body = r.bounds; any = true; }
            else body.Encapsulate(r.bounds);
        }

        float floor = transform.position.y - floorBelowRoot;
        float bottom = Mathf.Clamp(body.min.y - floor, 0f, 2f);
        float top = Mathf.Clamp(body.max.y - floor, 1.5f, 3.5f);
        float radius = Mathf.Clamp(Mathf.Max(body.extents.x, body.extents.z), 0.6f, 1.3f);

        // A bullet passing this far in front of the body (toward the camera) or behind it
        // still crosses the body on screen
        float towardCamera = Mathf.Max(0f, bulletHeight - bottom) * cot + radius;
        float awayFromCamera = Mathf.Max(0f, top - bulletHeight) * cot + radius;

        // The body's centre, in this box's camera-facing space
        Vector3 offset = Quaternion.Inverse(facing) * (body.center - transform.position);
        float low = -floorBelowRoot;
        float high = top + extraTop - floorBelowRoot;

        box.size = new Vector3(radius * 2f, high - low, towardCamera + awayFromCamera);
        box.center = new Vector3(offset.x, (low + high) / 2f, offset.z + (awayFromCamera - towardCamera) / 2f);
    }
}
