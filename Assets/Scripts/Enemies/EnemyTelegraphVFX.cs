using UnityEngine;

/// <summary>
/// Optional attack warning for enemies with a visible Renderer. Add on the enemy root.
/// Uses a MaterialPropertyBlock so shared materials aren't edited for every enemy.
/// </summary>
public class EnemyTelegraphVFX : MonoBehaviour
{
    public Renderer targetRenderer;
    public Color warningColor = new Color(1f, 0.15f, 0.08f, 1f);

    private MaterialPropertyBlock properties;
    private Color originalColor = Color.white;
    private bool supportsBaseColor;
    private bool supportsColor;

    private void Awake()
    {
        if (targetRenderer == null) targetRenderer = GetComponentInChildren<Renderer>();
        properties = new MaterialPropertyBlock();
        if (targetRenderer == null || targetRenderer.sharedMaterial == null) return;
        Material material = targetRenderer.sharedMaterial;
        supportsBaseColor = material.HasProperty("_BaseColor");
        supportsColor = material.HasProperty("_Color");
        if (supportsBaseColor) originalColor = material.GetColor("_BaseColor");
        else if (supportsColor) originalColor = material.GetColor("_Color");
    }

    public void SetWarning(bool isWarning)
    {
        if (targetRenderer == null) return;
        targetRenderer.GetPropertyBlock(properties);
        if (supportsBaseColor) properties.SetColor("_BaseColor", isWarning ? warningColor : originalColor);
        if (supportsColor) properties.SetColor("_Color", isWarning ? warningColor : originalColor);
        targetRenderer.SetPropertyBlock(properties);
    }

    private void OnDisable() { SetWarning(false); }
}
