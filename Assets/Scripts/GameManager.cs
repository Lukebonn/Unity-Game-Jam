using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerHealth player;

    public float SurvivedSeconds { get; private set; }

    private void Start()
    {
        // Works in any level scene: find the player if nobody dragged one in
        if (player == null) player = FindFirstObjectByType<PlayerHealth>();
    }

    private void Update()
    {
        // Time.deltaTime is 0 while the pause menu is open, so the clock stops too
        if (!player.isDead)
        {
            SurvivedSeconds += Time.deltaTime;
        }

        // (Esc is the pause menu now: see Menus/PauseMenu.cs)

#if UNITY_EDITOR
        // Debug key, left out of builds: Enter adds a die to the tray
        if (Input.GetKeyDown(KeyCode.Return))
        {
            FindFirstObjectByType<UnrolledDiceUI>().AddDie();
        }
#endif
    }

    // 187 seconds becomes "3:07"
    public static string FormatTime(float seconds)
    {
        int whole = Mathf.FloorToInt(seconds);
        return $"{whole / 60}:{whole % 60:00}";
    }
}
