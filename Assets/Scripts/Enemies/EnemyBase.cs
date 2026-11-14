
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

	[Header("Health Bar")]
	[Tooltip("How far above the enemy's centre its health bar floats, in metres")]
	public float healthBarHeight = 1.6f;

	// Lets the wave spawner know when an enemy dies.
	public static event Action<EnemyBase> OnAnyEnemyDied;

	// Every enemy that's alive, for the health bars over their heads
	public static readonly List<EnemyBase> All = new List<EnemyBase>();

	public bool IsDead => isDead;
	public float HealthFraction => (float)currentHealth / Mathf.Max(1, maxHealth);

	// Flying enemies (the mosquitoes) go straight over holes and never fall.
	// Walkers (the flies) follow the NavMesh around holes and over bridges.
	public virtual bool Flies => false;

	// This enemy's picture for the wave intro: Resources/EnemyIcons/<prefab name>
	public static Sprite IconFor(EnemyBase prefab) =>
		prefab != null ? Resources.Load<Sprite>("EnemyIcons/" + prefab.name) : null;

	protected int currentHealth;
	protected PlayerHealth player;
	protected bool isDead = false;

	private float flyHeight = 1f;

	protected virtual void OnEnable() => All.Add(this);
	protected virtual void OnDisable() => All.Remove(this);

	protected virtual void Awake()
	{
		currentHealth = Mathf.Max(1, maxHealth);

		// SeaGamez's textures, matched to the parts by name (see EnemySkins)
		EnemySkins.Apply(gameObject);

		// A trigger box lined up with the camera, so shots that look like hits are hits (see CameraHitbox)
		GameObject hitbox = new GameObject("Hitbox", typeof(BoxCollider), typeof(Rigidbody), typeof(CameraHitbox));
		hitbox.transform.SetParent(transform, false);
		hitbox.GetComponent<Rigidbody>().isKinematic = true;

		// Every part of the enemy goes on the Enemy layer, so the renderer can outline it through pillars
		int enemyLayer = LayerMask.NameToLayer("Enemy");
		if (enemyLayer >= 0)
		{
			foreach (Transform part in GetComponentsInChildren<Transform>(true))
				part.gameObject.layer = enemyLayer;
		}

		// Flyers hold the height they'd have standing on the floor
		CharacterController controller = GetComponent<CharacterController>();
		if (controller != null)
			flyHeight = controller.height / 2f - controller.center.y;
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

		if (currentHealth <= 0)
		{
			Die();
		}
	}

	protected virtual void Die()
	{
		if (isDead) return;

		isDead = true;

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

		SignalBus.InvokeEnemyDeath(transform.position);

		Destroy(gameObject);
	}

	// ---- Moving ----
	// One frame of movement at this velocity: walkers fall with gravity, flyers hold their height
	protected void Travel(CharacterController controller, Vector3 velocity)
	{
		if (Flies)
		{
			Vector3 step = velocity * Time.deltaTime;
			step.y = flyHeight - transform.position.y;
			controller.Move(step);
		}
		else
		{
			controller.SimpleMove(velocity);
		}
	}

	// Which way to go to reach a target: flyers go straight unless a pillar is in the way
	protected Vector3 SteerTo(Vector3 target)
	{
		if (Flies && !PillarBlocks(target))
		{
			Vector3 direction = target - transform.position;
			direction.y = 0f;
			return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
		}
		return PathDirectionTo(target);
	}

	// ---- Pathfinding around pillars ----
	// Uses the NavMesh baked on the Arena. Without a NavMesh, enemies just walk straight at the target.
	private NavMeshPath path;
	private readonly Vector3[] pathCorners = new Vector3[32];
	private int pathCornerCount;
	private int pathCorner;
	private float nextPathTime;

	// The direction to walk this frame to reach the target, going around pillars
	protected Vector3 PathDirectionTo(Vector3 target)
	{
		if (path == null) path = new NavMeshPath();

		if (Time.time >= nextPathTime)
		{
			nextPathTime = Time.time + 0.25f;   // re-plan four times a second
			pathCornerCount = 0;
			pathCorner = 0;
			if (NavMesh.SamplePosition(transform.position, out NavMeshHit from, 2f, NavMesh.AllAreas) &&
				NavMesh.SamplePosition(target, out NavMeshHit to, 2f, NavMesh.AllAreas) &&
				NavMesh.CalculatePath(from.position, to.position, NavMesh.AllAreas, path))
			{
				pathCornerCount = path.GetCornersNonAlloc(pathCorners);
			}
		}

		// Skip the corners we've already reached
		while (pathCorner < pathCornerCount && FlatDistance(transform.position, pathCorners[pathCorner]) < 0.6f)
			pathCorner++;

		Vector3 next = pathCorner < pathCornerCount ? pathCorners[pathCorner] : target;
		Vector3 direction = next - transform.position;
		direction.y = 0f;
		return direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.zero;
	}

	// True when a pillar stands between this enemy and the target
	protected bool PillarBlocks(Vector3 target)
	{
		int pillarMask = LayerMask.GetMask("Pillar");
		if (pillarMask == 0) return false;
		return Physics.Linecast(transform.position + Vector3.up * 0.5f, target + Vector3.up * 0.5f,
			pillarMask, QueryTriggerInteraction.Ignore);
	}

	private static float FlatDistance(Vector3 a, Vector3 b)
	{
		a.y = 0f;
		b.y = 0f;
		return Vector3.Distance(a, b);
	}
}
