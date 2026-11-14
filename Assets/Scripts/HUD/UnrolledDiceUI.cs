using UnityEngine;
using System.Collections.Generic;

// The dice tray at the top of the screen. Every kill adds a die that rolls, and a little
// d20 (WorldDie) pops out where the enemy died to show the number the die lands on.
public class UnrolledDiceUI : MonoBehaviour
{
    [SerializeField] private DiceUI diePrefab;
    [SerializeField] private WorldDie worldDiePrefab;   // optional: leave empty for no d20 in the world

    private readonly List<DiceUI> dice = new List<DiceUI>();

    // Dice in the tray, rolling or landed (the "+N" next to the level)
    public int Count => dice.Count;

    private void OnEnable()
    {
        SignalBus.OnEnemyDeath += HandleEnemyDeath;   // every kill gives a die
    }
    private void OnDisable()
    {
        SignalBus.OnEnemyDeath -= HandleEnemyDeath;
    }

    private void HandleEnemyDeath(Vector3 where)
    {
        DiceUI die = AddDie();

        if (worldDiePrefab != null)
        {
            WorldDie worldDie = Instantiate(worldDiePrefab, where, Quaternion.identity);
            worldDie.Follow(die);
        }
    }

    public DiceUI AddDie()
    {
        DiceUI die = Instantiate(diePrefab, transform);
        dice.Add(die);
        die.Roll();
        return die;
    }

    public bool TrySpendDie(out int rolled)
    {
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].HasLanded)
            {
                rolled = dice[i].Value;
                Destroy(dice[i].gameObject);
                dice.RemoveAt(i);
                return true;
            }
        }
        rolled = 0;
        return false;
    }
}
