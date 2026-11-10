using UnityEngine;
using System;

public class LevelUp : MonoBehaviour
{
    public static event Action<int> OnLevelUp;

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
