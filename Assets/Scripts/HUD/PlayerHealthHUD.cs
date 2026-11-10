using UnityEngine;

public class PlayerHealthHUD : MonoBehaviour
{
    public float Health, MaxHealth, Width, Height;
    [SerializeField] private RectTransform HealthBar;
    [SerializeField] private PlayerHealth PlayerHealth;

    void Start()
    {
        Health = PlayerHealth.Health();
    }

    void Update()
    {
        Health = PlayerHealth.Health();
        SetHealth();
    }

    void SetHealth()
    {
        float newWidth = (Health / MaxHealth) * Width;
        HealthBar.sizeDelta = new Vector2(newWidth , Height);
    }
}
