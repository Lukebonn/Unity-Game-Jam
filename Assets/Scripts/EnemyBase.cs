using System;
using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
	[Header("Base Enemy Stats")]
	public int maxHealth = 40;

	protected int currentHealth;
	protected PlayerHealth player;
	protected bool isDead = false;

    protected virtual void Awake()
	{
		currentHealth = maxHealth;
	}

	protected virtual void Start()
	{
		player = FindFirstObjectByType<PlayerHealth>();
	}

	public virtual void TakeDamage(int damage)
	{
		if (isDead || damage <= 0)
			return;

		currentHealth -= damage;

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

        OnEnemyDeath?.Invoke();

        Destroy(gameObject);
	}
}
