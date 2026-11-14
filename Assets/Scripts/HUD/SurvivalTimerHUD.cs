using UnityEngine;
using TMPro;

// Time survived so far, big at the bottom of the screen. GameManager owns the number.
public class SurvivalTimerHUD : MonoBehaviour
{
    [SerializeField] private GameManager game;
    [SerializeField] private TMP_Text timeText;

    private int shownSeconds = -1;

    private void Start()
    {
        // Works in any level scene: find the GameManager if nobody dragged one in
        if (game == null) game = FindFirstObjectByType<GameManager>();
    }

    private void Update()
    {
        int seconds = Mathf.FloorToInt(game.SurvivedSeconds);
        if (seconds != shownSeconds)
        {
            shownSeconds = seconds;
            timeText.text = GameManager.FormatTime(seconds);
        }
    }
}
