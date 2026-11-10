
using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
	public int maxHealth = 100;
	public int damageReductionPerHit = 0;

    // rolling stats (minus health)
    private int PSlevel = 1;
	//health is also here, but it should be noted that maxHealth is the PS variable
	private int PSstamina;
	private int PSmovement;
	private int PSrpm;
	private int PSarmor;
	private int PSdamage;


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

	//void OnGUI()
	//{
	//	GUI.Label(
	//		new Rect(20, 20, 250, 30),
	//		"Health: " + currentHealth + "/" + maxHealth
	//	);

	//	if (currentHealth == 0)
	//	{
	//		GUI.Label(
	//			new Rect(20, 50, 300, 30),
	//			"GAME OVER"
	//		);
	//	}
	//}

	private void OnEnable()
	{
		SignalBus.OnLevelUp += HandleLevelUp;
	}
    private void OnDisable()
    {
        SignalBus.OnLevelUp -= HandleLevelUp;
    }

    public float Health() { return currentHealth; }

	public void HandleLevelUp(int lvl)
    {
		PSlevel = lvl;

        Debug.Log($"[UI Receiver] Updating interface with score: {PSlevel}");
	}
}
