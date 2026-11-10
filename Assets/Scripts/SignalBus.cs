using UnityEngine;
using System;

public class SignalBus : MonoBehaviour
{
    //public static event Action<int> OnLevelUp;
    public static event Action OnLevelUp;
    public static event Action OnEnemyDeath;
    public bool DiceCocked = false;
    public static event Action OnLvlHealth;
    private int PSstamina;
    public static event Action OnLvlStamina;
    private int PSmovement;
    public static event Action OnLvlMovement;
    private int PSrpm;
    public static event Action OnLvlRPM;
    private int PSdamage;
    public static event Action OnLvlDamage;
    private int PSarmor;
    public static event Action OnLvlArmor;

    // Public setter functions

    public static void InvokeLevelUp()
    {
        OnLevelUp?.Invoke();
    }

    public static void InvokeEnemyDeath()
    {
        OnEnemyDeath?.Invoke();
    }
    public static void InvokeLvlHealth()
    {
        OnLevelUp?.Invoke();
    }

    public void InvokeLvlStamina()
    {
        OnLvlStamina?.Invoke();
    }

    public void InvokeLvlMovement()
    {
        OnLvlMovement?.Invoke();
    }

    public void InvokeLvlRPM()
    {
        OnLvlRPM?.Invoke();
    }

    public void InvokeLvlDamage()
    {
        OnLvlDamage?.Invoke();
    }

    public void InvokeLvlArmor()
    {
        OnLvlArmor?.Invoke();
    }


    //private int currLvl = 1;

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        currLvl += 1;
    //        OnLevelUp?.Invoke(currLvl);
    //    }
    //}
}