using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// After the last wave is cleared: "YOU WIN", the run's stats, and the game stops.
// Goes on the Canvas, NOT on the panel: a switched-off object can't hear events.
public class WinUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup panelFade;
    [SerializeField] private TMP_Text stats;
    [SerializeField] private GameManager game;
    [SerializeField] private WaveSpawner waves;
    [SerializeField] private LevelUp levelUp;

    // PauseMenu ignores Esc once you've won
    public static bool HasWon { get; private set; }

    private void Awake()
    {
        panel.SetActive(false);
        HasWon = false;
    }

    private void Start()
    {
        // Works in any level scene: find these if nobody dragged them in
        if (game == null) game = FindFirstObjectByType<GameManager>();
        if (waves == null) waves = FindFirstObjectByType<WaveSpawner>();
        if (levelUp == null) levelUp = FindFirstObjectByType<LevelUp>();
        if (waves != null) waves.AllWavesCleared += HandleWin;
    }

    private void OnDestroy()
    {
        if (waves != null) waves.AllWavesCleared -= HandleWin;
    }

    private void HandleWin()
    {
        HasWon = true;
        StartCoroutine(ShowPanel());
    }

    private IEnumerator ShowPanel()
    {
        stats.text =
            $"Time {GameManager.FormatTime(game.SurvivedSeconds)}\n" +
            $"Waves {waves.CurrentWave}\n" +
            $"Kills {waves.KillCount}\n" +
            $"Level {levelUp.Level}";

        // Everything stops; the panel fades in on real time
        Time.timeScale = 0f;
        panel.SetActive(true);
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime)
        {
            panelFade.alpha = t;
            yield return null;
        }
        panelFade.alpha = 1f;
    }

    // Hooked up to the buttons' On Click () lists in the Inspector
    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);   // scene 0 in Build Profiles is the main menu
    }
}
