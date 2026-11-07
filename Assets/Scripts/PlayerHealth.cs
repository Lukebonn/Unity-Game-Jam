
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
	public int maxHealth = 100;
	public int damageReductionPerHit = 0;

	private int currentHealth;

	void Awake()
	{
		currentHealth = maxHealth;
	}

	public void TakeDamage(int incomingDamage)
	{
		if (currentHealth <= 0) return;

		int damage = Mathf.Max(
			0,
			incomingDamage - damageReductionPerHit
		);

		currentHealth = Mathf.Max(
			0,
			currentHealth - damage
		);

		Debug.Log("Player HP: " + currentHealth);

		if (currentHealth == 0)
		{
			Debug.Log("GAME OVER!");
			GetComponent<PlayerCombat>().enabled = false;
		}
	}

	void OnGUI()
	{
		GUI.Label(
			new Rect(20, 20, 250, 30),
			"Health: " + currentHealth + "/" + maxHealth
		);

		if (currentHealth == 0)
		{
			GUI.Label(
				new Rect(20, 50, 300, 30),
				"GAME OVER"
			);
		}
	}
}
