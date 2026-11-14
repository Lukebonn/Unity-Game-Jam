using System.Text;
using UnityEngine;
using TMPro;

// Writes the 1-6 stat list into a text: the short version for the HUD's left panel,
// or the long one for the Upgrades screen before a run.
// The order matches the keys: key 1 is Names[0], key 6 is Names[5] (see LevelUp.ApplyStat).
public class StatList : MonoBehaviour
{
    public static readonly string[] Names = { "Health", "Stamina", "Speed", "Fire rate", "Damage", "Armor" };

    // Two or three words each, for the HUD
    public static readonly string[] Hints = { "max HP", "sprint + dash", "walk + dash", "shots per second", "per bullet", "less damage taken" };

    // A sentence each, for the Upgrades screen
    public static readonly string[] Details =
    {
        "More max HP. The new part of the bar arrives full.",
        "A longer stamina bar, for sprinting and dashing.",
        "Walk faster. Your dash goes further too.",
        "More bullets every second.",
        "Every bullet hits harder.",
        "Take less damage from every hit (up to 75%).",
    };

    // Matching the HUD: red health bar, green stamina bar, gold speedometer, blue and orange gun icons
    public static readonly Color[] Colors =
    {
        new Color(0.9f, 0.3f, 0.3f),
        new Color(0.4f, 0.8f, 0.45f),
        new Color(1f, 0.82f, 0.35f),
        new Color(0.2f, 0.8f, 1f),
        new Color(1f, 0.55f, 0.15f),
        new Color(0.75f, 0.8f, 0.92f),
    };

    [SerializeField] private TMP_Text text;
    [SerializeField] private bool detailed;   // off: HUD list, on: Upgrades screen

    private void Awake()
    {
        var list = new StringBuilder();
        for (int i = 0; i < Names.Length; i++)
        {
            if (i > 0) list.Append("\n<size=35%> </size>\n");   // a small gap between stats

            string colour = ColorUtility.ToHtmlStringRGB(Colors[i]);
            if (detailed)
            {
                list.Append($"<color=#{colour}><b>{i + 1}   {Names[i]}</b></color>\n");
                list.Append($"<size=75%>{Details[i]}</size>");
            }
            else
            {
                list.Append($"<color=#{colour}><size=150%><b>{i + 1}</b></size>  <b>{Names[i]}</b></color>\n");
                list.Append($"<size=80%><color=#B8B8B8>{Hints[i]}</color></size>");
            }
        }
        text.text = list.ToString();
    }
}
