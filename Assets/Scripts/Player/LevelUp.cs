using UnityEngine;
using System;

public class LevelUp : MonoBehaviour
{
    [SerializeField] private UnrolledDiceUI diceTray;

    private void Update()
    {
        for (int key = 1; key <= 6; key++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + key))
            {
                if (diceTray.TrySpendDie(out int rolled))
                {
                    ApplyStat(key, rolled);
                }
                else
                {
                    Debug.Log("No dice to apply stats to");
                }
            }
            
        }
    }

    private void ApplyStat(int stat, int rolled)
    {
        Debug.Log($"Applying a die that rolled: {rolled} on stat {stat}");
        switch (stat)
        {
            case 1: SignalBus.InvokeLvlHealth(rolled); break;
            case 2: Debug.Log("Event Stamina"); break;
            case 3: Debug.Log("Event Move Speed"); break;
            case 4: Debug.Log("Event Bullets Per Second"); break;
            case 5: Debug.Log("Event Damage Per Bullet"); break;
            case 6: Debug.Log("Event Received Damage Per Bullet"); break;
        }
    }
}