using UnityEngine;
using System;

public class SignalBus : MonoBehaviour
{
    public static event Action<int> OnLevelUp;
    public static event Action OnEnemyDeath;


    private int currLvl = 1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currLvl += 1;
            OnLevelUp?.Invoke(currLvl);
        }
    }
}