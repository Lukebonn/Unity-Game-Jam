
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class TestEnemy : EnemyBase
{
	[Header("Movement")]
	public float moveSpeed = 2.5f;
	public float stopDistance = 1.2f;

	[Header("Attack")]
	public float attackRange = 1.6f;
	public int damagePerHit = 10;
	public float attackInterval = 1f;

	private CharacterController controller;
	private float nextAttackTime;

	protected override void Awake()
	{
		base.Awake();

		controller = GetComponent<CharacterController>();
	}

	void Update()
	{
		if (player == null || isDead)
			return;

		Vector3 direction =
			player.transform.position - transform.position;

		direction.y = 0;

		float distance = direction.magnitude;

		if (distance > 0.01f)
		{
			direction.Normalize();

			transform.rotation =
				Quaternion.LookRotation(direction);
		}

		if (distance > stopDistance)
		{
			controller.SimpleMove(
				PathDirectionTo(player.transform.position) * moveSpeed
			);
		}
		else
		{
			controller.SimpleMove(Vector3.zero);
		}

		if (distance <= attackRange &&
			Time.time >= nextAttackTime)
		{
			nextAttackTime = Time.time + attackInterval;

			player.TakeDamage(damagePerHit);
		}
	}
}
