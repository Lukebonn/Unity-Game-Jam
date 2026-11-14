using UnityEngine;

// The player's lucky number, picked on the main menu. Rolling it gives a star (see LuckyEffects).
// PlayerPrefs saves it on the player's PC, so it's remembered next launch.
public static class LuckyNumber
{
    public const int Max = 6;   // one number per face of a d6
    private const string SaveKey = "LuckyNumber";

    public static int Value
    {
        // Clamped on the way out too, so a number saved back when the dice were d20s still works
        get => Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, 6), 1, Max);
        set => PlayerPrefs.SetInt(SaveKey, Mathf.Clamp(value, 1, Max));
    }
}
