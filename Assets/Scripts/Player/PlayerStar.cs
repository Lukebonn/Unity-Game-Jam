using UnityEngine;

// The lucky-number star (think Mario Kart): every stat doubles for a few seconds and the
// player glows like a disco ball. PlayerCombat, PlayerHealth and PlayerStamina read Multiplier.
public class PlayerStar : MonoBehaviour
{
    [SerializeField] private Material discoMaterial;     // Remus/Disco Aura (optional)
    [SerializeField] private float auraSize = 3.2f;      // metres across
    [SerializeField] private float hueSpeed = 1.5f;      // colour cycles per second

    public bool IsActive => Time.time < endTime;
    public float Multiplier => IsActive ? 2f : 1f;
    public float SecondsLeft => Mathf.Max(0f, endTime - Time.time);

    private float endTime = -1f;
    private GameObject aura;
    private Light glow;
    private Renderer[] body;
    private MaterialPropertyBlock block;
    private bool tinted;

    private void Start()
    {
        block = new MaterialPropertyBlock();

        // The player's own model, to flash it through the colours (not the bullet line)
        var parts = new System.Collections.Generic.List<Renderer>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
            if (r is MeshRenderer || r is SkinnedMeshRenderer) parts.Add(r);
        body = parts.ToArray();

        // A see-through mirror ball around the player
        if (discoMaterial != null)
        {
            aura = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            aura.name = "Star Aura";
            Destroy(aura.GetComponent<Collider>());
            aura.transform.SetParent(transform, false);
            aura.transform.localScale = Vector3.one * auraSize;
            MeshRenderer auraRenderer = aura.GetComponent<MeshRenderer>();
            auraRenderer.sharedMaterial = discoMaterial;
            auraRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            aura.SetActive(false);
        }

        // Coloured light spilling onto the floor around the player
        glow = new GameObject("Star Light").AddComponent<Light>();
        glow.transform.SetParent(transform, false);
        glow.transform.localPosition = Vector3.up * 1.5f;
        glow.type = LightType.Point;
        glow.range = 8f;
        glow.intensity = 5f;
        glow.enabled = false;
    }

    // Called by LuckyEffects when a die lands on the lucky number
    public void Begin(float seconds)
    {
        endTime = Time.time + seconds;
    }

    private void Update()
    {
        bool on = IsActive;
        // The last two seconds blink, so you can tell it's about to run out
        bool show = on && (SecondsLeft > 2f || Mathf.Repeat(Time.time * 8f, 1f) < 0.6f);

        if (aura != null && aura.activeSelf != show) aura.SetActive(show);
        if (glow.enabled != show) glow.enabled = show;

        if (!on)
        {
            if (tinted)
            {
                foreach (Renderer r in body) if (r != null) r.SetPropertyBlock(null);
                tinted = false;
            }
            return;
        }

        Color hue = Color.HSVToRGB(Mathf.Repeat(Time.time * hueSpeed, 1f), 0.75f, 1f);
        glow.color = hue;
        foreach (Renderer r in body)
        {
            if (r == null) continue;
            r.GetPropertyBlock(block);
            block.SetColor("_BaseColor", Color.Lerp(Color.white, hue, 0.75f));
            r.SetPropertyBlock(block);
        }
        tinted = true;
    }
}
