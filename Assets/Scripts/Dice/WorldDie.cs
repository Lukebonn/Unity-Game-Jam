using UnityEngine;
using TMPro;

// The little d20 that pops out where an enemy dies. It spins while its die in the tray
// rolls, then shows the number that die landed on above itself, and fades away.
public class WorldDie : MonoBehaviour
{
    [SerializeField] private Transform model;            // the d20
    [SerializeField] private TMP_Text number;            // world-space text above it
    [SerializeField] private float hoverHeight = 0.8f;   // metres above the floor
    [SerializeField] private float spinSpeed = 720f;     // degrees a second while rolling
    [SerializeField] private float showTime = 1.6f;      // seconds the number stays up
    [SerializeField] private Color numberColor = Color.white;
    [SerializeField] private Color luckyColor = new Color(1f, 0.8f, 0.2f);

    private DiceUI source;
    private Camera cam;
    private Vector3 spinAxis;
    private Vector3 modelScale;
    private float spawnTime;
    private float landedTime = -1f;   // -1 while still rolling

    // The dice tray calls this right after spawning it
    public void Follow(DiceUI die)
    {
        source = die;
    }

    private void Start()
    {
        cam = Camera.main;
        spinAxis = Random.onUnitSphere;
        modelScale = model.localScale;
        spawnTime = Time.time;
        number.gameObject.SetActive(false);

        // Sit on the floor under the spot where the enemy died
        int floorMask = ~LayerMask.GetMask("Enemy", "Player");
        if (Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, 4f, floorMask, QueryTriggerInteraction.Ignore))
            transform.position = hit.point;
    }

    private void Update()
    {
        if (landedTime < 0f)
        {
            // Rolling: tumble and bounce while the tray die rolls
            model.Rotate(spinAxis, spinSpeed * Time.deltaTime, Space.World);
            float bounce = Mathf.Abs(Mathf.Sin((Time.time - spawnTime) * 9f)) * 0.35f;
            model.localPosition = Vector3.up * (hoverHeight + bounce);

            if (source == null || Time.time - spawnTime > 10f)
                Destroy(gameObject);   // its tray die is gone
            else if (source.HasLanded)
                Land();
            return;
        }

        // Landed: pop the number in, then fade everything out
        float shown = Time.time - landedTime;
        number.transform.localScale = Vector3.one * Mathf.Min(1f, 0.3f + shown * 5f);
        float fade = Mathf.Clamp01((showTime - shown) / 0.4f);
        number.alpha = fade;
        model.localScale = modelScale * fade;

        if (shown >= showTime)
            Destroy(gameObject);
    }

    private void Land()
    {
        landedTime = Time.time;
        model.localPosition = Vector3.up * hoverHeight;
        number.text = source.Value.ToString();
        number.color = source.IsLucky ? luckyColor : numberColor;
        number.gameObject.SetActive(true);
    }

    private void LateUpdate()
    {
        // The number always faces the camera
        if (cam != null)
            number.transform.rotation = cam.transform.rotation;
    }
}
