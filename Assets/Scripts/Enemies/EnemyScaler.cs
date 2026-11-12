using UnityEngine;

/// <summary>
/// Applies increasing health, damage, and movement speed to a NEW enemy instance.
/// Supports the eight enemy classes.
/// Attach this component to the same object as WaveSpawner.
/// </summary>
public class EnemyScaler : MonoBehaviour
{
	[Header("Growth per Wave (percentage points)")]
	[Range(0, 40)] public int healthPercentPerWave = 8;
	[Range(0, 40)] public int damagePercentPerWave = 5;
	[Range(0f, 20f)] public float speedPercentPerWave = 1.5f;

	[Header("Limits to Keep Late Waves Playable")]
	[Min(1f)] public float maximumHealthMultiplier = 5f;
	[Min(1f)] public float maximumDamageMultiplier = 3f;
	[Min(1f)] public float maximumSpeedMultiplier = 1.75f;

	public void Apply(EnemyBase enemy, int wave)
	{
		if (enemy == null) return;
		int wavesPassed = Mathf.Max(0, wave - 1);

		float healthFactor = Mathf.Min(maximumHealthMultiplier,
			1f + wavesPassed * healthPercentPerWave / 100f);
		float damageFactor = Mathf.Min(maximumDamageMultiplier,
			1f + wavesPassed * damagePercentPerWave / 100f);
		float speedFactor = Mathf.Min(maximumSpeedMultiplier,
			1f + wavesPassed * speedPercentPerWave / 100f);

		// EnemyBase.SetMaxHealth also fills current health to the new maximum.
		enemy.SetMaxHealth(Mathf.Max(1, Mathf.RoundToInt(enemy.maxHealth * healthFactor)));

		if (enemy is EnemyMeleeAI melee)
		{
			melee.damagePerHit = Mathf.Max(1, Mathf.RoundToInt(melee.damagePerHit * damageFactor));
			melee.moveSpeed *= speedFactor;
		}
		else if (enemy is EnemyDashAI dash)
		{
			dash.dashDamage = Mathf.Max(1, Mathf.RoundToInt(dash.dashDamage * damageFactor));
			dash.moveSpeed *= speedFactor;
			// Keep the dash warning/dodge window stable as difficulty rises.
		}
		else if (enemy is EnemyRangedAI ranged)
		{
			ranged.damagePerProjectile = Mathf.Max(1,
				Mathf.RoundToInt(ranged.damagePerProjectile * damageFactor));
			ranged.moveSpeed *= speedFactor;
		}
	}
}
