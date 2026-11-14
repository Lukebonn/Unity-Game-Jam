using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SpawnEntry
{
	public EnemyBase prefab;
	[Min(1)] public int unlockWave = 1;
	[Min(1)] public int weight = 1; // Higher weight = more likely to spawn.
}

/// <summary>
/// Endless, timed waves: escalating counts, faster spawns, weighted enemy unlocks.
/// Requires the EnemyBase class
/// IMPORTANT: Disable the old SurvivalDirector when using this spawner.
/// </summary>
[RequireComponent(typeof(EnemyScaler))]
public class WaveSpawner : MonoBehaviour
{
	[Header("References")]
	public PlayerHealth player;
	public SpawnEntry[] enemyTypes;
	public Transform[] spawnPoints;
	public LayerMask groundMask = ~0;

	[Header("Hand-made waves (optional)")]
	[Tooltip("Set this to choose exactly how many of each enemy come per wave, and how fast. Leave it empty to use the automatic waves below.")]
	public WavePlan wavePlan;

	[Header("Automatic Wave Size & Timing (used when Wave Plan is empty)")]
	[Min(1)] public int startingEnemies = 5;
	[Min(0)] public int extraEnemiesPerWave = 2;
	[Min(5f)] public float waveTimeLimit = 30f;
	[Min(0f)] public float breakBetweenWaves = 3f;
	[Min(0.05f)] public float startingSpawnInterval = 0.8f;
	[Min(0f)] public float secondsFasterPerWave = 0.035f;
	[Min(0.05f)] public float minimumSpawnInterval = 0.25f;
	[Min(1)] public int maxAlive = 25;

	[Header("Spawn Locations")]
	[Min(0f)] public float minDistanceFromPlayer = 10f;
	[Tooltip("Spawn points farther than this from the player are skipped, so a big map doesn't spawn enemies miles away. 0 = no limit.")]
	[Min(0f)] public float maxSpawnPointDistance = 40f;
	[Min(1f)] public float minRandomRadius = 13f;
	[Min(1f)] public float maxRandomRadius = 20f;

	public int CurrentWave { get; private set; }
	public int KillCount { get; private set; }
	public int EnemiesAlive => alive.Count;

	private readonly HashSet<EnemyBase> alive = new HashSet<EnemyBase>();
	private EnemyScaler scaler;

	private void Awake()
	{
		if (player == null)
			player = FindFirstObjectByType<PlayerHealth>();
		scaler = GetComponent<EnemyScaler>();
	}

	private void OnEnable() => EnemyBase.OnAnyEnemyDied += OnEnemyDied;
	private void OnDisable() => EnemyBase.OnAnyEnemyDied -= OnEnemyDied;

	private void Start()
	{
		if (player == null)
		{
			Debug.LogError("RemusWaveSpawner: assign a PlayerHealth component.", this);
			enabled = false;
			return;
		}
		StartCoroutine(WaveLoop());
	}

	private void OnEnemyDied(EnemyBase deadEnemy)
	{
		// Only count enemies created by this spawner.
		if (alive.Remove(deadEnemy)) KillCount++;
	}

	private IEnumerator WaveLoop()
	{
		yield return new WaitForSeconds(1f);

		while (player != null && !player.isDead)
		{
			CurrentWave++;
			List<EnemyBase> toSpawn = BuildWave(out float interval, out float timeLimit, out float breakAfter);
			if (toSpawn.Count == 0)
			{
				Debug.LogError("RemusWaveSpawner: wave " + CurrentWave + " has no enemies. " +
							   "Fill in the Wave Plan, or add an enemy with unlockWave = 1.", this);
				yield break;
			}
			float waveEndTime = Time.time + timeLimit;

			Debug.Log($"Wave {CurrentWave}: spawning up to {toSpawn.Count} enemies.");
			int spawned = 0;

			// Spawn all scheduled enemies, unless the wave timer or player life runs out.
			while (spawned < toSpawn.Count && Time.time < waveEndTime && !player.isDead)
			{
				alive.RemoveWhere(enemy => enemy == null);
				if (alive.Count >= maxAlive)
				{
					yield return new WaitForSeconds(0.1f);
					continue;
				}

				EnemyBase prefab = toSpawn[spawned];
				spawned++;
				if (prefab == null) continue;   // an empty slot in the Wave Plan

				Vector3 position = FindSpawnPosition();
				EnemyBase spawnedEnemy = Instantiate(prefab, position, Quaternion.identity);
				scaler.Apply(spawnedEnemy, CurrentWave);
				alive.Add(spawnedEnemy);
				yield return new WaitForSeconds(interval);
			}

			// Begin the next wave when the arena is clear OR the timer expires.
			// Survivors stay alive, keeping constant pressure on the player.
			while (Time.time < waveEndTime && !player.isDead)
			{
				alive.RemoveWhere(enemy => enemy == null);
				if (alive.Count == 0) break;
				yield return null;
			}

			if (!player.isDead)
				yield return new WaitForSeconds(breakAfter);
		}
	}

	// This wave's enemies: from the Wave Plan if there is one, otherwise the automatic settings.
	private List<EnemyBase> BuildWave(out float interval, out float timeLimit, out float breakAfter)
	{
		List<EnemyBase> list = new List<EnemyBase>();

		if (wavePlan != null && wavePlan.waves != null && wavePlan.waves.Length > 0)
		{
			// After the last hand-made wave, keep repeating it (EnemyScaler still makes it tougher).
			WavePlan.Wave wave = wavePlan.waves[Mathf.Min(CurrentWave, wavePlan.waves.Length) - 1];
			if (wave.enemies != null)
			{
				foreach (WavePlan.EnemyCount group in wave.enemies)
				{
					if (group == null || group.prefab == null) continue;
					for (int i = 0; i < group.count; i++) list.Add(group.prefab);
				}
			}
			Shuffle(list);   // mix the enemy types up
			interval = wave.secondsBetweenSpawns;
			timeLimit = wave.timeLimit;
			breakAfter = wave.breakAfter;
			return list;
		}

		int targetCount = Mathf.Max(1, startingEnemies + (CurrentWave - 1) * extraEnemiesPerWave);
		for (int i = 0; i < targetCount; i++)
		{
			EnemyBase prefab = PickEnemy();
			if (prefab != null) list.Add(prefab);
		}
		interval = Mathf.Max(minimumSpawnInterval,
			startingSpawnInterval - (CurrentWave - 1) * secondsFasterPerWave);
		timeLimit = waveTimeLimit;
		breakAfter = breakBetweenWaves;
		return list;
	}

	private static void Shuffle(List<EnemyBase> list)
	{
		for (int i = list.Count - 1; i > 0; i--)
		{
			int j = UnityEngine.Random.Range(0, i + 1);
			(list[i], list[j]) = (list[j], list[i]);
		}
	}

	private EnemyBase PickEnemy()
	{
		if (enemyTypes == null) return null;
		int totalWeight = 0;
		foreach (SpawnEntry entry in enemyTypes)
			if (entry != null && entry.prefab != null && entry.unlockWave <= CurrentWave)
				totalWeight += Mathf.Max(1, entry.weight);

		if (totalWeight == 0) return null;
		int roll = UnityEngine.Random.Range(0, totalWeight);
		foreach (SpawnEntry entry in enemyTypes)
		{
			if (entry == null || entry.prefab == null || entry.unlockWave > CurrentWave)
				continue;
			roll -= Mathf.Max(1, entry.weight);
			if (roll < 0) return entry.prefab;
		}
		return null;
	}

	private Vector3 FindSpawnPosition()
	{
		if (spawnPoints != null && spawnPoints.Length > 0)
		{
			// Prefer designated positions a safe distance away from the player.
			for (int i = 0; i < spawnPoints.Length * 3; i++)
			{
				Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
				if (point == null) continue;
				float distance = Vector3.Distance(point.position, player.transform.position);
				if (distance >= minDistanceFromPlayer &&
					(maxSpawnPointDistance <= 0f || distance <= maxSpawnPointDistance))
					return point.position;
			}
		}

		// Fallback: pick somewhere around the player, then snap down to the floor.
		Vector2 direction = UnityEngine.Random.insideUnitCircle.normalized;
		if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
		float inner = Mathf.Max(minRandomRadius, minDistanceFromPlayer);
		float outer = Mathf.Max(inner, maxRandomRadius);
		Vector3 position = player.transform.position +
			new Vector3(direction.x, 0f, direction.y) * UnityEngine.Random.Range(inner, outer);

		if (Physics.Raycast(position + Vector3.up * 30f, Vector3.down,
			out RaycastHit hit, 100f, groundMask, QueryTriggerInteraction.Ignore))
			return hit.point + Vector3.up * 0.1f;
		return position;
	}
}
