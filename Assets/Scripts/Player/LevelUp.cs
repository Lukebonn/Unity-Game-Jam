using UnityEngine;
using System;

public class LevelUp : MonoBehaviour
{
    private bool diceCocked;
    public static event Action<int> OnLevelUp;

    private int currLvl = 1;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            currLvl += 1;
            OnLevelUp?.Invoke(currLvl);
        }
        if (diceCocked)
        {
            void Update()
            {
                for (int i = 1; i <= 6; i++)
                {
                    if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                    {
                        TriggerEvent(i);
                    }
                }
            }

            void TriggerEvent(int number)
            {
                switch (number)
                {
                    case 1:
                        SignalBus.InvokeLvlHealth();
                        break;

                    case 2:
                        Debug.Log("Event Stamina");
                        break;

                    case 3:
                        Debug.Log("Event Move Speed");
                        break;

                    case 4:
                        Debug.Log("Event Bullets Per Second");
                        break;

                    case 5:
                        Debug.Log("Event Damage Per Bullet");
                        break;

                    case 6:
                        Debug.Log("Event Received Damage Per Bullet");
                        break;
                }
            }
        }
    }
}