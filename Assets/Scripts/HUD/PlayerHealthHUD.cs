using UnityEngine;

// The health bar, with the armor bar joined on to its end in blue once you have armor.
// Armor takes hits first (see PlayerHealth), so the blue part empties before the red does.
public class PlayerHealthHUD : MonoBehaviour
{
    [SerializeField] private RectTransform HealthBar;    // the red fill (child)
    [SerializeField] private RectTransform ArmorBack;    // the slot for armor, right after the health bar (child)
    [SerializeField] private RectTransform ArmorBar;     // the blue fill (child of ArmorBack)
    [SerializeField] private PlayerHealth PlayerHealth;
    [SerializeField] private float Width = 200f;         // bar length at the starting max HP
    [SerializeField] private float Height = 72f;
    [SerializeField] private float MaxWidth = 600f;      // health + armor stop growing here so they stay on screen

    private RectTransform background;

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
        // One HP or one armor point is the same width, so the two bars line up
        float perPoint = Width / PlayerHealth.StartingMaxHealth;
        float healthWidth = PlayerHealth.maxHealth * perPoint;
        float armorWidth = PlayerHealth.MaxArmor * perPoint;

        // Too long for the screen? Shrink both together
        float total = healthWidth + armorWidth;
        if (total > MaxWidth)
        {
            float squeeze = MaxWidth / total;
            healthWidth *= squeeze;
            armorWidth *= squeeze;
        }

        background.sizeDelta = new Vector2(healthWidth, Height);
        HealthBar.sizeDelta = new Vector2(healthWidth * PlayerHealth.CurrentHealth / PlayerHealth.maxHealth, Height);

        // The blue bar doesn't exist until the first armor upgrade
        bool hasArmor = PlayerHealth.MaxArmor > 0f;
        if (ArmorBack.gameObject.activeSelf != hasArmor) ArmorBack.gameObject.SetActive(hasArmor);
        if (hasArmor)
        {
            ArmorBack.sizeDelta = new Vector2(armorWidth, Height);
            ArmorBar.sizeDelta = new Vector2(armorWidth * PlayerHealth.CurrentArmor / PlayerHealth.MaxArmor, Height);
        }
    }
}
