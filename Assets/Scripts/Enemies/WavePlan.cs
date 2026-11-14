using System;
using UnityEngine;

// One level's waves: how many of each enemy come, and how fast.
// Make one with Assets > Create > Remus > Wave Plan, then drag it onto the WaveManager's Wave Plan field.
[CreateAssetMenu(fileName = "WavePlan", menuName = "Remus/Wave Plan")]
public class WavePlan : ScriptableObject
{
    [Serializable]
    public class EnemyCount
    {
        public EnemyBase prefab;
        [Min(0)] public int count = 3;
    }

    [Serializable]
    public class Wave
    {
        public EnemyCount[] enemies;
        [Min(0.05f)] public float secondsBetweenSpawns = 0.8f;
        [Min(5f)] public float timeLimit = 30f;   // the next wave starts when the arena is clear or this runs out
        [Min(0f)] public float breakAfter = 3f;   // pause before the next wave
    }

    [Tooltip("Wave 1 is element 0. After the last wave, the last one repeats, and EnemyScaler keeps making it tougher.")]
    public Wave[] waves;
}
