using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    public float maxStamina = 100f;
    public int levelUpIncrementPrct = 2;   // same rule as health: +2% of starting max per point
    public float regenPerSecond = 30f;
    public float regenDelay = 0.75f;       // seconds after the last use before it refills
    [Range(0f, 1f)] public float recoverAt = 0.25f;   // after running dry, sprint again at 25%

    public float CurrentStamina { get; private set; }
    public float StartingMaxStamina { get; private set; }

    private float lastUsedTime;
    private bool exhausted;

    private void Awake()
    {
        StartingMaxStamina = maxStamina;
        CurrentStamina = maxStamina;
    }

    private void OnEnable()
    {
        SignalBus.OnLvlStamina += HandleStaminaLvlUp;
    }

    private void OnDisable()
    {
        SignalBus.OnLvlStamina -= HandleStaminaLvlUp;
    }

    private void Update()
    {
        if (Time.time - lastUsedTime < regenDelay) return;

        CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + regenPerSecond * Time.deltaTime);

        if (exhausted && CurrentStamina >= maxStamina * recoverAt)
        {
            exhausted = false;
        }
    }

    // Spends stamina unless we're exhausted. Returns false when it can't.
    public bool TryUse(float amount)
    {
        if (exhausted) return false;

        CurrentStamina -= amount;
        lastUsedTime = Time.time;

        if (CurrentStamina <= 0f)
        {
            CurrentStamina = 0f;
            exhausted = true;   // stops sprint flickering on and off at 0
        }
        return true;
    }

    private void HandleStaminaLvlUp(int roll)
    {
        float gained = StartingMaxStamina * (levelUpIncrementPrct / 100f) * roll;
        maxStamina += gained;
        CurrentStamina += gained;

        Debug.Log($"Stamina +{gained}: {CurrentStamina}/{maxStamina}");
    }
}
