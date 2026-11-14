using UnityEngine;
using TMPro;

public class LevelUp : MonoBehaviour
{
    [SerializeField] private UnrolledDiceUI diceTray;
    [SerializeField] private TMP_Text levelValue;     // the number in the top-right badge
    [SerializeField] private TMP_Text pendingValue;   // "+3" beside it while 3 dice wait in the tray

    public int Level { get; private set; } = 1;

    private int shownPending = -1;

    private void Start()
    {
        levelValue.text = Level.ToString();
    }

    private void Update()
    {
        ShowPending();

        // No spending dice from the pause menu
        if (PauseMenu.IsPaused) return;

        for (int key = 1; key <= 6; key++)
        {
            // Top-row number keys or the numpad (the GDD asks for the numpad)
            if (Input.GetKeyDown(KeyCode.Alpha0 + key) || Input.GetKeyDown(KeyCode.Keypad0 + key))
            {
                SpendAllDice(key);
            }
        }
    }

    // "+N" counts every die in the tray, rolling or landed. Hidden at 0.
    private void ShowPending()
    {
        if (pendingValue == null) return;

        int pending = diceTray.Count;
        if (pending == shownPending) return;

        shownPending = pending;
        pendingValue.text = "+" + pending;
        pendingValue.gameObject.SetActive(pending > 0);
    }

    // Puts every landed die into one stat. Dice that are still rolling stay in the tray.
    private void SpendAllDice(int stat)
    {
        int spent = 0;
        while (diceTray.TrySpendDie(out int rolled))
        {
            ApplyStat(stat, rolled);
            spent++;
        }

        if (spent == 0)
        {
            Debug.Log("No landed dice to spend");
            return;
        }

        Level += spent;   // +1 level per die
        levelValue.text = Level.ToString();
    }

    // Same order as StatList.Names: 1 Health, 2 Stamina, 3 Speed, 4 Fire rate, 5 Damage, 6 Armor
    private void ApplyStat(int stat, int rolled)
    {
        Debug.Log($"Applying a die that rolled {rolled} to stat {stat}");
        switch (stat)
        {
            case 1: SignalBus.InvokeLvlHealth(rolled); break;
            case 2: SignalBus.InvokeLvlStamina(rolled); break;
            case 3: SignalBus.InvokeLvlMovement(rolled); break;
            case 4: SignalBus.InvokeLvlRPM(rolled); break;
            case 5: SignalBus.InvokeLvlDamage(rolled); break;
            case 6: SignalBus.InvokeLvlArmor(rolled); break;
        }
    }
}
