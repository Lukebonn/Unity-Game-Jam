using System.Text;
using UnityEngine;
using TMPro;

// The six stats in key order: key 1 is Names[0], key 6 is Names[5] (see LevelUp.ApplyStat).
// StatPanelHUD uses these for the left panel. As a component, this writes the full list into a
// text for the Upgrades screen before a run (or the short list, with detailed off).
// The stat names use the text's own font (Caesar Dressing); the descriptions switch to Crimson Text.
public class StatList : MonoBehaviour
{
    public static readonly string[] Names = { "Health", "Stamina", "Speed", "Fire rate", "Damage", "Armor" };

    // Two or three words each, for the HUD
    public static readonly string[] Hints = { "max HP", "sprint longer", "faster feet", "shoot faster", "hit harder", "blue shield bar" };

    // A sentence each, for the Upgrades screen
    public static readonly string[] Details =
    {
        "More max HP. The new part of the bar arrives full.",
        "A longer sprint. You start with about one second of it.",
        "A higher top speed, walking and sprinting.",
        "About 10% more bullets per second every time.",
        "Every bullet hits harder.",
        "A blue bar after your health that takes the hits first. It doesn't grow back.",
    };

    // Matching the HUD: red health bar, green stamina bar, gold speedometer, blue armor bar
    public static readonly Color[] Colors =
    {
        new Color(0.9f, 0.3f, 0.3f),
        new Color(0.4f, 0.8f, 0.45f),
        new Color(1f, 0.82f, 0.35f),
        new Color(0.8f, 0.55f, 1f),
        new Color(1f, 0.55f, 0.15f),
        new Color(0.35f, 0.65f, 1f),
    };

    private const string BodyFont = "<font=\"CrimsonText SDF\">";

    [SerializeField] private TMP_Text text;
    [SerializeField] private bool detailed = true;   // on: Upgrades screen, off: short list

    private void Awake()
    {
        var list = new StringBuilder();
        for (int i = 0; i < Names.Length; i++)
        {
            if (i > 0) list.Append("\n<size=35%> </size>\n");   // a small gap between stats

            string colour = ColorUtility.ToHtmlStringRGB(Colors[i]);
            list.Append($"<color=#{colour}>{i + 1}   {Names[i]}</color>\n");
            list.Append(detailed
                ? $"{BodyFont}<size=85%>{Details[i]}</size></font>"
                : $"{BodyFont}<size=80%><color=#B8B8B8>{Hints[i]}</color></size></font>");
        }
        text.text = list.ToString();
    }
}
