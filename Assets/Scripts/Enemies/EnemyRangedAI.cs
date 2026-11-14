using System.Collections;
using UnityEngine;

/// <summary>Shared keep-distance shooter with telegraphed burst attacks.</summary>
[RequireComponent(typeof(CharacterController))]
public abstract class EnemyRangedAI : EnemyBase
{
    [Header("Positioning")]
    public float moveSpeed = 3f;
    public float minRange = 5f;
    public float idealRange = 10f;
    public float maxRange = 14f;
    public float strafeWeight = 0.4f;

    [Header("Shooting")]
    public int damagePerProjectile = 7;
    public float projectileSpeed = 14f;
    public float fireCooldown = 2f;
    public float windupSeconds = 0.6f;
    public int bulletsPerBurst = 3;
    public float burstInterval = 0.17f;
    public float spreadDegrees = 7f;

    [Header("Optional References")]
    public Transform muzzle;
    public GameObject projectilePrefab;
    public Animator animator;

    private CharacterController controller;
    private EnemyTelegraphVFX warningVFX;
    private bool firing;
    private float nextShotTime;
    private int strafeDirection;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<CharacterController>();
        warningVFX = GetComponent<EnemyTelegraphVFX>();
        strafeDirection = Random.value > 0.5f ? 1 : -1;
    }

    protected virtual void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (isDead || player == null || player.isDead) return;

        Vector3 toward = player.transform.position - transform.position;
        toward.y = 0f;
        float distance = toward.magnitude;
        Vector3 direction = distance > 0.01f ? toward / distance : transform.forward;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction),
            Time.deltaTime * 10f);

        bool blocked = PillarBlocks(player.transform.position);
        Vector3 movement = Vector3.zero;
        if (!firing)
        {
            // Too far away, or a pillar in the way: follow the NavMesh path closer
            if (distance > idealRange || blocked) movement = PathDirectionTo(player.transform.position);
            else if (distance < minRange) movement = -direction;
            else movement = Vector3.Cross(Vector3.up, direction) * strafeDirection * strafeWeight;
        }
        controller.SimpleMove(movement * moveSpeed);

        // Only shoot along a clear line, so shots aren't wasted on pillars
        if (distance <= maxRange && distance >= minRange * 0.65f &&
            Time.time >= nextShotTime && !firing && !blocked)
        {
            nextShotTime = Time.time + Mathf.Max(0.1f, fireCooldown);
            StartCoroutine(FireRoutine());
        }
    }

    private IEnumerator FireRoutine()
    {
        firing = true;
        if (animator != null) animator.SetTrigger("Windup");
        if (warningVFX != null) warningVFX.SetWarning(true);
        yield return new WaitForSeconds(Mathf.Max(0f, windupSeconds));
        if (isDead || player == null || player.isDead)
        {
            if (warningVFX != null) warningVFX.SetWarning(false);
            firing = false;
            yield break;
        }

        if (warningVFX != null) warningVFX.SetWarning(false);
        if (animator != null) animator.SetTrigger("Attack");
        for (int i = 0; i < Mathf.Max(1, bulletsPerBurst); i++)
        {
            if (isDead || player == null || player.isDead) break;
            FireOneProjectile();
            if (i < bulletsPerBurst - 1)
                yield return new WaitForSeconds(Mathf.Max(0.01f, burstInterval));
        }
        firing = false;
    }

    private void FireOneProjectile()
    {
        Vector3 origin = muzzle != null ? muzzle.position : transform.position + Vector3.up * 0.7f + transform.forward * 0.7f;
        Vector3 aim = player.transform.position + Vector3.up * 0.6f - origin;
        if (aim.sqrMagnitude < 0.001f) return;
        aim = Quaternion.Euler(0f, Random.Range(-spreadDegrees, spreadDegrees), 0f) * aim.normalized;

        GameObject bullet;
        if (projectilePrefab != null)
            bullet = Instantiate(projectilePrefab, origin, Quaternion.LookRotation(aim));
        else
        {
            // No prefab is required to test the game jam build.
            bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bullet.name = "Enemy Bullet (fallback)";
            bullet.transform.position = origin;
            bullet.transform.localScale = Vector3.one * 0.25f;
        }

        EnemyProjectile projectile = bullet.GetComponent<EnemyProjectile>();
        if (projectile == null) projectile = bullet.AddComponent<EnemyProjectile>();
        projectile.Initialize(aim, projectileSpeed, damagePerProjectile);
    }
}
