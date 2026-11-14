using UnityEngine;
using TMPro;

// The panel down the left of the screen: the six stats with their keys, and a diamond beside each
// with how many dice it has had. Names, hints and colours come from StatList; the numbers from LevelUp.
public class StatPanelHUD : MonoBehaviour
{
    [SerializeField] private LevelUp levelUp;
    [SerializeField] private TMP_Text[] names = new TMP_Text[6];    // "1  Health" ... "6  Armor"
    [SerializeField] private TMP_Text[] hints = new TMP_Text[6];    // "max HP" ...
    [SerializeField] private TMP_Text[] levels = new TMP_Text[6];   // the number in each diamond

    private readonly int[] shown = { -1, -1, -1, -1, -1, -1 };

    private void Awake()
    {
        for (int i = 0; i < 6; i++)
        {
            names[i].text = $"{i + 1}  {StatList.Names[i]}";
            names[i].color = StatList.Colors[i];
            hints[i].text = StatList.Hints[i];
        }
    }

    private void Start()
    {
        // Works in any level scene: find LevelUp if nobody dragged it in
        if (levelUp == null) levelUp = FindFirstObjectByType<LevelUp>();
    }

    private void Update()
    {
        if (levelUp == null) return;

        for (int i = 0; i < 6; i++)
        {
            int level = levelUp.StatLevel(i + 1);
            if (level == shown[i]) continue;
            shown[i] = level;
            levels[i].text = level.ToString();
        }
    }
}
