using UnityEngine;
using TMPro;

// Bottom right: how many enemies you've killed this run. WaveSpawner owns the number.
public class KillCountHUD : MonoBehaviour
{
    [SerializeField] private WaveSpawner waves;
    [SerializeField] private TMP_Text killsValue;

    private int shownKills = -1;

    private void Start()
    {
        // Works in any level scene: find the WaveManager if nobody dragged one in
        if (waves == null) waves = FindFirstObjectByType<WaveSpawner>();
    }

    private void Update()
    {
        if (waves == null) return;

        // Only touch the text when the number changes
        int kills = waves.KillCount;
        if (kills != shownKills)
        {
            shownKills = kills;
            killsValue.text = kills.ToString();
        }
    }
}
