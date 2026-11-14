using UnityEngine;

// What a lucky roll does. None means the roll wasn't lucky.
public enum LuckyEffect { None, KillAll, TimesTen, Instakill }

// The player's lucky number, picked on the main menu.
// PlayerPrefs saves it on the player's PC, so it's remembered next launch.
public static class LuckyNumber
{
    public const int Max = 20;   // one number per face of a d20
    private const string SaveKey = "LuckyNumber";

    public static int Value
    {
        get => PlayerPrefs.GetInt(SaveKey, 7);
        set => PlayerPrefs.SetInt(SaveKey, Mathf.Clamp(value, 1, Max));
    }
}
