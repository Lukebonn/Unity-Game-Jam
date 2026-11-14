using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
	public float maxHealth = 100f;
	public int levelUpIncrementPrct = 5;    // each rolled point = +5% of the STARTING max HP

	[Header("Armor: a blue bar after the health bar that soaks up hits first")]
	public float armorPerPoint = 8f;        // armor added per rolled point (it doesn't grow back)

	public bool isDead;

	// Anyone can read these, only PlayerHealth can change them.
	public float CurrentHealth { get; private set; }
	public float StartingMaxHealth { get; private set; }
	public float MaxArmor { get; private set; }
	public float CurrentArmor { get; private set; }

	private PlayerStar star;

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

		// A lucky star halves every hit (the same as having double the health)
		if (star == null) star = GetComponent<PlayerStar>();
		float damage = incomingDamage / (star != null ? star.Multiplier : 1f);

		// Armor soaks up the hit first; whatever is left comes off health
		float soaked = Mathf.Min(CurrentArmor, damage);
		CurrentArmor -= soaked;
		damage -= soaked;
		CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);

		if (CurrentHealth <= 0f)
			Die();
	}

	// Falling into a hole: lose half of the health you have right now (armor doesn't help)
	public void LoseHalfHealth()
	{
		if (isDead) return;
		CurrentHealth *= 0.5f;
	}

	private void Die()
	{
		isDead = true;
		GetComponent<PlayerCombat>().enabled = false;
		SignalBus.InvokePlayerDied();
	}

	private void HandleHlthLvlUp(int roll)
	{
		float gained = StartingMaxHealth * (levelUpIncrementPrct / 100f) * roll;
		maxHealth += gained;
		CurrentHealth += gained;   // the new part of the bar arrives full
	}

	private void HandleArmorLvlUp(int roll)
	{
		float gained = roll * armorPerPoint;
		MaxArmor += gained;
		CurrentArmor += gained;    // the new armor arrives full too
	}
}
