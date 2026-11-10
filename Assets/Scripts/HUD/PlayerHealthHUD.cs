using UnityEngine;

public class PlayerHealthHUD : MonoBehaviour
{
    public float Health, MaxHealth, Width, Height;
    private float levelUpIncrementPrct;
    [SerializeField] private RectTransform HealthBar;
    [SerializeField] private PlayerHealth PlayerHealth;

    void Start()
    {
        (Health, MaxHealth, levelUpIncrementPrct) = PlayerHealth.Health();
    }

    void Update()
    {
        (Health, MaxHealth, levelUpIncrementPrct) = PlayerHealth.Health();
        SetHealth();
    }

    void SetHealth()
    {
        Width += (Width * (levelUpIncrementPrct / 100));
        float newWidth = (Health / MaxHealth) * Width;
        HealthBar.sizeDelta = new Vector2(newWidth , Height);
    }
}
