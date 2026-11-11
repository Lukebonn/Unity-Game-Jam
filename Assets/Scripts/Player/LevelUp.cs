using UnityEngine;
using System;

public class LevelUp : MonoBehaviour
{
    private int diceCocked = 0;
    public static event Action<int> OnLevelUp;

    private int currLvl = 1;

    private void OnEnable()
    {
        SignalBus.OnLvlHealth += HandleLvlUp;
    }
    private void OnDisable()
    {
        SignalBus.OnLvlHealth -= HandleLvlUp;
    }

    private void HandleLvlUp()
    {
        diceCocked++;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    currLvl += 1;
        //    OnLevelUp?.Invoke(currLvl);
        //}
        if (diceCocked > 0)
        {
            for (int i = 1; i <= 6; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    TriggerEvent(i);
                }
            }

            void TriggerEvent(int number)
            {
                switch (number)
                {
                    case 1:
                        SignalBus.InvokeLvlHealth();
                        diceCocked--;
                        break;

                    case 2:
                        Debug.Log("Event Stamina");
                        diceCocked--;
                        break;

                    case 3:
                        Debug.Log("Event Move Speed");
                        diceCocked--;
                        break;

                    case 4:
                        Debug.Log("Event Bullets Per Second");
                        diceCocked--;
                        break;

                    case 5:
                        Debug.Log("Event Damage Per Bullet");
                        diceCocked--;
                        break;

                    case 6:
                        Debug.Log("Event Received Damage Per Bullet");
                        diceCocked--;
                        break;
                }
            }
        }
    }
}