using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
	public float maxHealth = 100f;
	public int levelUpIncrementPrct = 2;    // each rolled point = +2% of the STARTING max HP

	[Header("Armor (damage reduction)")]
	public float armorPercentPerPoint = 0.35f;  // d20 rolls are big, so under 1% per point
	public int maxArmorPercent = 75;            // cap, so armor can never make you unkillable

	public bool isDead;

	// Anyone can read these, only PlayerHealth can change them.
	public float CurrentHealth { get; private set; }
	public float StartingMaxHealth { get; private set; }
	public int ArmorPercent { get; private set; }

	private void Awake()
	{
		StartingMaxHealth = maxHealth;
		CurrentHealth = maxHealth;
		isDead = false;
	}

	private void OnEnable()
	{
		SignalBus.OnLvlHealth += HandleHlthLvlUp;
		SignalBus.OnLvlArmor += HandleArmorLvlUp;
	}

	private void OnDisable()
	{
		SignalBus.OnLvlHealth -= HandleHlthLvlUp;
		SignalBus.OnLvlArmor -= HandleArmorLvlUp;
	}

	public void TakeDamage(int incomingDamage)
	{
		if (isDead) return;

		// 12% armor means you take 88% of the hit
		float damage = incomingDamage * (1f - ArmorPercent / 100f);
		CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

		Debug.Log($"Player HP: {CurrentHealth}");

		if (CurrentHealth <= 0f)
		{
			isDead = true;
			Debug.Log("GAME OVER!");
			GetComponent<PlayerCombat>().enabled = false;
			SignalBus.InvokePlayerDied();
		}
	}

	private void HandleHlthLvlUp(int roll)
	{
		float gained = StartingMaxHealth * (levelUpIncrementPrct / 100f) * roll;
		maxHealth += gained;
		CurrentHealth += gained;   // the new part of the bar arrives full

		Debug.Log($"Health +{gained}: {CurrentHealth}/{maxHealth}");
	}

	private void HandleArmorLvlUp(int roll)
	{
		int gained = Mathf.RoundToInt(roll * armorPercentPerPoint);
		ArmorPercent = Mathf.Min(maxArmorPercent, ArmorPercent + gained);

		Debug.Log($"Armor now {ArmorPercent}%");
	}
}
