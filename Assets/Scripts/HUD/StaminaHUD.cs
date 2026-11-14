using UnityEngine;

public class StaminaHUD : MonoBehaviour
{
    [SerializeField] private RectTransform StaminaBar;   // the green fill (child)
    [SerializeField] private PlayerStamina PlayerStamina;
    [SerializeField] private float Width = 200f;
    [SerializeField] private float Height = 72f;
    [SerializeField] private float MaxWidth = 600f;

    private RectTransform background;

    private void Awake()
    {
        background = (RectTransform)transform;
    }

    private void Start()
    {
        // Works in any level scene: find the player if nobody dragged one in
        if (PlayerStamina == null) PlayerStamina = FindFirstObjectByType<PlayerStamina>();
    }

    private void Update()
    {
        float max = PlayerStamina.maxStamina;
        float fullWidth = Mathf.Min(MaxWidth, Width * max / PlayerStamina.StartingMaxStamina);

        background.sizeDelta = new Vector2(fullWidth, Height);
        StaminaBar.sizeDelta = new Vector2(fullWidth * PlayerStamina.CurrentStamina / max, Height);
    }
}
