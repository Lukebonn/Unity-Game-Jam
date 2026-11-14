using UnityEngine;

// Carries out the lucky effects that happen in the world.
// (TimesTen is about the die itself, so DiceUI handles it.)
public class LuckyEffects : MonoBehaviour
{
    [SerializeField] private PlayerCombat player;
    [SerializeField] private float instakillSeconds = 10f;

    private void OnEnable() => SignalBus.OnDieLanded += HandleDieLanded;
    private void OnDisable() => SignalBus.OnDieLanded -= HandleDieLanded;

    private void Start()
    {
        // Works in any level scene: find the player if nobody dragged one in
        if (player == null) player = FindFirstObjectByType<PlayerCombat>();
    }

    private void HandleDieLanded(int value, LuckyEffect effect)
    {
        if (effect == LuckyEffect.None) return;

        Debug.Log($"LUCKY {value}: {effect}");

        switch (effect)
        {
            case LuckyEffect.KillAll:
                // Every enemy alive dies. The wave keeps going.
                foreach (EnemyBase enemy in FindObjectsByType<EnemyBase>(FindObjectsSortMode.None))
                {
                    enemy.TakeDamage(enemy.maxHealth);
                }
                break;

            case LuckyEffect.Instakill:
                player.StartInstakill(instakillSeconds);
                break;
        }
    }
}
