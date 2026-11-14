using UnityEngine;
using TMPro;

public class PlayerHealthHUD : MonoBehaviour
{
    [SerializeField] private RectTransform HealthBar;    // the red fill (child)
    [SerializeField] private PlayerHealth PlayerHealth;
    [SerializeField] private float Width = 200f;         // bar length at the starting max HP
    [SerializeField] private float Height = 12f;
    [SerializeField] private float MaxWidth = 600f;      // stop growing here so it stays on screen

    [Header("Armor, beside the bar")]
    [SerializeField] private GameObject Armor;
    [SerializeField] private TMP_Text ArmorValue;

    private RectTransform background;
    private int shownArmor = -1;

    private void Awake()
    {
        background = (RectTransform)transform;
    }

    private void Start()
    {
        // Works in any level scene: find the player if nobody dragged one in
        if (PlayerHealth == null) PlayerHealth = FindFirstObjectByType<PlayerHealth>();
    }

    private void Update()
    {
        // Only READ from PlayerHealth. All the health maths lives there.
        float max = PlayerHealth.maxHealth;
        float fullWidth = Mathf.Min(MaxWidth, Width * max / PlayerHealth.StartingMaxHealth);

        background.sizeDelta = new Vector2(fullWidth, Height);
        HealthBar.sizeDelta = new Vector2(fullWidth * PlayerHealth.CurrentHealth / max, Height);

        // Only touch the text when the number changes
        int armor = PlayerHealth.ArmorPercent;
        if (armor != shownArmor)
        {
            shownArmor = armor;
            ArmorValue.text = armor.ToString();
            Armor.SetActive(armor > 0);   // the shield shows up with the first armor point
        }
    }
}
