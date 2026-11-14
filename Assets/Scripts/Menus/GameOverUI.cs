using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

// Goes on the Canvas, NOT on the panel: a switched-off object can't hear events.
public class GameOverUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private CanvasGroup panelFade;
    [SerializeField] private TMP_Text stats;
    [SerializeField] private GameManager game;
    [SerializeField] private WaveSpawner waves;
    [SerializeField] private LevelUp levelUp;
    [SerializeField] private float delay = 1.5f;   // a souls-like beat before "YOU DIED"

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void Start()
    {
        // Works in any level scene: find these if nobody dragged them in
        if (game == null) game = FindFirstObjectByType<GameManager>();
        if (waves == null) waves = FindFirstObjectByType<WaveSpawner>();
        if (levelUp == null) levelUp = FindFirstObjectByType<LevelUp>();
    }

    private void OnEnable() => SignalBus.OnPlayerDied += HandlePlayerDied;
    private void OnDisable() => SignalBus.OnPlayerDied -= HandlePlayerDied;

    private void HandlePlayerDied()
    {
        StartCoroutine(ShowPanel());
    }

    private IEnumerator ShowPanel()
    {
        yield return new WaitForSeconds(delay);

        stats.text =
            $"Time {GameManager.FormatTime(game.SurvivedSeconds)}\n" +
            $"Wave {waves.CurrentWave}\n" +
            $"Kills {waves.KillCount}\n" +
            $"Level {levelUp.Level}";

        panel.SetActive(true);
        for (float t = 0f; t < 1f; t += Time.deltaTime)
        {
            panelFade.alpha = t;
            yield return null;
        }
        panelFade.alpha = 1f;
    }

    // Hooked up to the buttons' On Click () lists in the Inspector
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(0);   // scene 0 in Build Profiles is the main menu
    }
}
