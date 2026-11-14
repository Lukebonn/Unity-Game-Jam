using UnityEngine;
using System;

public class SignalBus : MonoBehaviour
{
    // Dice and the run
    public static event Action<int, bool> OnDieLanded;          // a die finished rolling: its number, and whether it was your lucky number
    public static event Action OnPlayerDied;
    public static event Action<Vector3> OnEnemyDeath;            // raised by Cash's EnemyBase, with where the enemy died

    // Stats: one event per stat. The int is the number the die rolled.
    public static event Action<int> OnLvlHealth;
    public static event Action<int> OnLvlStamina;
    public static event Action<int> OnLvlMovement;
    public static event Action<int> OnLvlRPM;
    public static event Action<int> OnLvlDamage;
    public static event Action<int> OnLvlArmor;

    public static void InvokeDieLanded(int value, bool lucky) => OnDieLanded?.Invoke(value, lucky);
    public static void InvokePlayerDied() => OnPlayerDied?.Invoke();
    public static void InvokeEnemyDeath(Vector3 where) => OnEnemyDeath?.Invoke(where);

    public static void InvokeLvlHealth(int roll) => OnLvlHealth?.Invoke(roll);
    public static void InvokeLvlStamina(int roll) => OnLvlStamina?.Invoke(roll);
    public static void InvokeLvlMovement(int roll) => OnLvlMovement?.Invoke(roll);
    public static void InvokeLvlRPM(int roll) => OnLvlRPM?.Invoke(roll);
    public static void InvokeLvlDamage(int roll) => OnLvlDamage?.Invoke(roll);
    public static void InvokeLvlArmor(int roll) => OnLvlArmor?.Invoke(roll);
}
