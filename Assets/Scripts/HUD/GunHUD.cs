using UnityEngine;
using TMPro;

// Sketch spot C: DOOM Eternal style readouts for fire rate and damage.
public class GunHUD : MonoBehaviour
{
    [SerializeField] private PlayerCombat player;
    [SerializeField] private TMP_Text fireRateValue;   // rounds per minute
    [SerializeField] private TMP_Text damageValue;     // damage per bullet
    [SerializeField] private Color instakillColor = new Color(1f, 0.25f, 0.1f);

    private int shownRpm = -1;
    private int shownDamage = -1;
    private Color normalColor;

    private void Awake()
    {
        normalColor = damageValue.color;   // whatever colour it has in the Inspector
    }

    private void Start()
    {
        // Works in any level scene: find the player if nobody dragged one in
        if (player == null) player = FindFirstObjectByType<PlayerCombat>();
    }

    private void Update()
    {
        int rpm = Mathf.RoundToInt(player.bulletsPerSecond * 60f);
        if (rpm != shownRpm)
        {
            shownRpm = rpm;
            fireRateValue.text = rpm.ToString();
        }

        if (player.damagePerBullet != shownDamage)
        {
            shownDamage = player.damagePerBullet;
            damageValue.text = shownDamage.ToString();
        }

        // Lucky instakill: the damage number glows while it lasts
        damageValue.color = player.IsInstakill ? instakillColor : normalColor;
    }
}
