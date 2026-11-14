using UnityEngine;

// Rolling your lucky number gives the player a star: every stat doubled for a few seconds,
// disco glow included (see PlayerStar). Goes on the GameManager.
public class LuckyEffects : MonoBehaviour
{
    [SerializeField] private PlayerStar star;
    [SerializeField] private float starSeconds = 10f;

    private void OnEnable() => SignalBus.OnDieLanded += HandleDieLanded;
    private void OnDisable() => SignalBus.OnDieLanded -= HandleDieLanded;

    private void Start()
    {
        // Works in any level scene: find the player's star if nobody dragged one in
        // (PlayerCombat adds one to the player in Awake if it's missing)
        if (star == null) star = FindFirstObjectByType<PlayerStar>();
    }

    private void HandleDieLanded(int value, bool lucky)
    {
        if (lucky && star != null)
            star.Begin(starSeconds);
    }
}
