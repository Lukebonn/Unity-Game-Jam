
using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
	[Header("Base Enemy Stats")]
	[Min(1)]
	public int maxHealth = 40;

	[Header("Dice Drop")]
	[SerializeField] private GameObject dicePrefab;
	[SerializeField]
	private Vector3 diceSpawnOffset =
		new Vector3(0f, 1f, 0f);

	// Lets the wave spawner know when an enemy dies.
	public static event Action<EnemyBase> OnAnyEnemyDied;

	public bool IsDead => isDead;

	protected int currentHealth;
	protected PlayerHealth player;
	protected bool isDead = false;

	protected virtual void Awake()
	{
		currentHealth = Mathf.Max(1, maxHealth);
	}

	protected virtual void Start()
	{
		player = FindFirstObjectByType<PlayerHealth>();
	}


	public void SetMaxHealth(int newMaxHealth, bool refill = true)
	{
		maxHealth = Mathf.Max(1, newMaxHealth);

		if (refill)
		{
			currentHealth = maxHealth;
		}
		else
		{
			currentHealth = Mathf.Min(currentHealth, maxHealth);
		}
	}

	public virtual void TakeDamage(int damage)
	{
		if (isDead || damage <= 0)
			return;

		currentHealth = Mathf.Max(0, currentHealth - damage);

		Debug.Log($"{gameObject.name} HP: {currentHealth}");

		if (currentHealth <= 0)
		{
			Die();
		}
	}

	protected virtual void Die()
	{
		if (isDead) return;

		isDead = true;

		Debug.Log($"{gameObject.name} died!");

		// OPTIONAL: Spawn a physical die when killed.
		if (dicePrefab != null)
		{
			Instantiate(
				dicePrefab,
				transform.position + diceSpawnOffset,
				UnityEngine.Random.rotation
			);
		}

		//Notify the wave spawner.
		OnAnyEnemyDied?.Invoke(this);

		SignalBus.InvokeEnemyDeath();

		Destroy(gameObject);
	}
}
