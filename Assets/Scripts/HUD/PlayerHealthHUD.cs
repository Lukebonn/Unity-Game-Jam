using UnityEngine;

public class PlayerHealthHUD : MonoBehaviour
{
    public float Health, MaxHealth, Width, Height;
    private float levelUpIncrementPrct;
    [SerializeField] private RectTransform HealthBar;
    [SerializeField] private PlayerHealth PlayerHealth;

    void Update()
    {
        (Health, MaxHealth, levelUpIncrementPrct) = PlayerHealth.Health();
        SetBar();
    }
    private void OnEnable()
    {
        SignalBus.OnLvlHealth += ExpandBar;
    }
    private void OnDisable()
    {
        SignalBus.OnLvlHealth -= ExpandBar;
    }

    void ExpandBar(int stat)
    {
        Width += (Width * ((levelUpIncrementPrct * stat) / 100f));
    }
    void SetBar()
    {
        float newWidth = (Health / MaxHealth) * Width;
        HealthBar.sizeDelta = new Vector2(newWidth , Height);
    }
}
