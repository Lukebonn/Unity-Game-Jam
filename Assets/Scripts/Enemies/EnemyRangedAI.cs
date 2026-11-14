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
    [Tooltip("Bullets (and their red lanes on the ground) go no further than this")]
    public float shotReach = 30f;

    [Header("Optional References")]
    public Transform muzzle;
    public GameObject projectilePrefab;
    public Animator animator;

    // Shooters are mosquitoes: they fly over holes
    public override bool Flies => true;

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
            // Too far away, or a pillar in the way: fly closer (around the pillar)
            if (distance > idealRange || blocked) movement = SteerTo(player.transform.position);
            else if (distance < minRange) movement = -direction;
            else movement = Vector3.Cross(Vector3.up, direction) * strafeDirection * strafeWeight;
        }
        Travel(controller, movement * moveSpeed);

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

        GameObject bullet = projectilePrefab != null
            ? Instantiate(projectilePrefab, origin, Quaternion.LookRotation(aim))
            : MakeBullet(origin);

        // The bullet's path, up to the first pillar it would hit: shown on the ground as a red lane.
        // The bullet takes the time it needs to fly down the lane; be in the red when it passes and it hits you.
        float reach = shotReach;
        int solid = ~LayerMask.GetMask("Enemy", "Player", "Ignore Raycast");
        if (Physics.Raycast(origin, aim, out RaycastHit hit, reach, solid, QueryTriggerInteraction.Ignore))
            reach = hit.distance;
        GroundTelegraph lane = GroundTelegraph.Create();
        if (lane != null) lane.ShowStrip(bullet, origin, aim, reach, 0.9f);

        EnemyProjectile projectile = bullet.GetComponent<EnemyProjectile>();
        if (projectile == null) projectile = bullet.AddComponent<EnemyProjectile>();
        projectile.Initialize(aim, projectileSpeed, damagePerProjectile, lane, reach);
    }

    // No prefab is required to test the game jam build: a glowing ball that hits anything within
    // half the red lane's width of its path
    private static GameObject MakeBullet(Vector3 origin)
    {
        GameObject bullet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        bullet.name = "Enemy Bullet";
        bullet.layer = LayerMask.NameToLayer("Ignore Raycast");   // the player's shots pass through it
        bullet.transform.position = origin;
        bullet.transform.localScale = Vector3.one * 0.4f;
        bullet.GetComponent<SphereCollider>().radius = 0.45f / 0.4f;   // 0.45 m: matches the lane
        Material glow = Resources.Load<Material>("EnemyBullet");
        if (glow != null) bullet.GetComponent<MeshRenderer>().sharedMaterial = glow;
        bullet.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return bullet;
    }
}
