using UnityEngine;
using UnityEngine.SceneManagement;

// Esc pauses the run: time stops and a see-through menu opens over the game.
// Goes on the Canvas, next to GameOverUI.
public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject panel;          // the see-through pause screen
    [SerializeField] private GameObject[] subPanels;    // Controls and Credits: Esc closes these first
    [SerializeField] private PlayerHealth player;

    // Other scripts check this to ignore input while the game is paused
    public static bool IsPaused { get; private set; }

    private void Awake()
    {
        panel.SetActive(false);
        SetPaused(false);
    }

    private void Start()
    {
        // Works in any level scene: find the player if nobody dragged one in
        if (player == null) player = FindFirstObjectByType<PlayerHealth>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (player != null && player.isDead) return;   // the game-over screen has its own buttons

        // Back out of Controls or Credits first
        foreach (GameObject subPanel in subPanels)
        {
            if (subPanel.activeSelf)
            {
                subPanel.SetActive(false);
                return;
            }
        }

        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        panel.SetActive(true);
        SetPaused(true);
    }

    // Hooked up to the buttons' On Click () lists in the Inspector
    public void Resume()
    {
        foreach (GameObject subPanel in subPanels) subPanel.SetActive(false);
        panel.SetActive(false);
        SetPaused(false);
    }

    public void BackToMenu()
    {
        SetPaused(false);
        SceneManager.LoadScene(0);   // scene 0 in Build Profiles is the main menu
    }

    private static void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;   // stops movement, enemies, waves, the clock and rolling dice
        AudioListener.pause = paused;
    }

    private void OnDestroy()
    {
        // Never carry a frozen game into the next scene
        if (IsPaused) SetPaused(false);
    }
}
