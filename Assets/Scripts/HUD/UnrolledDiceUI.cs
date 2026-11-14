using UnityEngine;
using System.Collections.Generic;

public class UnrolledDiceUI : MonoBehaviour
{
    [SerializeField] private DiceUI diePrefab;

    private readonly List<DiceUI> dice = new List<DiceUI>();

    private void OnEnable()
    {
        SignalBus.OnEnemyDeath += AddDie;   // every kill gives a d20
    }
    private void OnDisable()
    {
        SignalBus.OnEnemyDeath -= AddDie;
    }

    public void AddDie()
    {
        DiceUI die = Instantiate(diePrefab, transform);
        dice.Add(die);
        die.Roll();
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
