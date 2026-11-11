
using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
	public float maxHealth = 100f;
	public int damageReductionPerHit = 0; //now a legacy variable (read below)
	public int levelUpIncrementPrct;

    // rolling stats (minus health ofc^)
    private int PSlevel = 1;
	//health is also here, but it should be noted that maxHealth is the PS variable
	private int PSstamina;
	private int PSmovement;
	private int PSrpm;
	private int PSarmor;
	private int PSdamage;


	private float currentHealth;

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
		SignalBus.OnLvlHealth += HandleHlthLvlUp;
	}
    private void OnDisable()
    {
        SignalBus.OnLvlHealth -= HandleHlthLvlUp;
    }

    public (float num1, float num2, float num3) Health() { return (currentHealth, maxHealth, levelUpIncrementPrct); }

	public void HandleHlthLvlUp(int stat)
    {
		PSlevel += 1;

		maxHealth += maxHealth * (levelUpIncrementPrct / 100f);

        Debug.Log($"Player leveled up to: {PSlevel}, Player health now: {maxHealth}");
	}
}
