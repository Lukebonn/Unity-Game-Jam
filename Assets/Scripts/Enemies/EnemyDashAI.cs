using System.Collections;
using UnityEngine;

/// <summary>Telegraphed dash to the player's position at the START of the windup.</summary>
[RequireComponent(typeof(CharacterController))]
public abstract class EnemyDashAI : EnemyBase
{
    [Header("Chase")]
    public float moveSpeed = 4.8f;
    public float dashTriggerRange = 7f;
    public float comfortableDistance = 4.2f;

    [Header("Dash")]
    public int dashDamage = 12;
    public float windupSeconds = 0.4f;
    public float dashSpeed = 17f;
    public float maxDashDistance = 9f;
    public float hitRadius = 1.1f;
    public float recoverySeconds = 0.5f;
    public float dashCooldown = 2f;
    public Animator animator;

    // Dashers are mosquitoes: they fly over holes
    public override bool Flies => true;

    private CharacterController controller;
    private EnemyTelegraphVFX warningVFX;
    private bool busy;
    private float nextDashTime;

    protected override void Awake()
    {
        base.Awake();
        controller = GetComponent<CharacterController>();
        warningVFX = GetComponent<EnemyTelegraphVFX>();
    }

    protected virtual void Reset()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (isDead || player == null || player.isDead || busy) return;
        Vector3 toward = player.transform.position - transform.position;
        toward.y = 0f;
        float distance = toward.magnitude;

        // Fly straight at the player, around pillars when one is in the way
        Vector3 move = distance > comfortableDistance ? SteerTo(player.transform.position) : Vector3.zero;
        Vector3 face = move.sqrMagnitude > 0.01f ? move : toward;
        if (face.sqrMagnitude > 0.0025f) transform.rotation = Quaternion.LookRotation(face);

        Travel(controller, move * moveSpeed);
        // Only dash along a clear straight line, never into a pillar
        if (distance <= dashTriggerRange && Time.time >= nextDashTime && !PillarBlocks(player.transform.position))
        {
            nextDashTime = Time.time + Mathf.Max(0.1f, dashCooldown);
            StartCoroutine(DashRoutine(player.transform.position));
        }
    }

    private IEnumerator DashRoutine(Vector3 target)
    {
        busy = true;
        if (animator != null) animator.SetTrigger("Windup");
        if (warningVFX != null) warningVFX.SetWarning(true);

        // Aim at where the player is now, and mark the whole dash on the ground
        Vector3 travel = target - transform.position;
        travel.y = 0f;
        Vector3 direction = travel.sqrMagnitude > 0.01f ? travel.normalized : transform.forward;
        transform.rotation = Quaternion.LookRotation(direction);
        float dashLength = Mathf.Min(maxDashDistance, travel.magnitude + 1f);
        GroundTelegraph lane = GroundTelegraph.Create();
        if (lane != null) lane.ShowStrip(this, transform.position, direction, dashLength, hitRadius * 2f);

        for (float t = 0f; t < windupSeconds; t += Time.deltaTime)
        {
            if (lane != null) lane.SetFill(t / windupSeconds);
            yield return null;
        }

        if (isDead || player == null || player.isDead)
        {
            if (warningVFX != null) warningVFX.SetWarning(false);
            if (lane != null) Destroy(lane.gameObject);
            busy = false;
            yield break;
        }

        if (warningVFX != null) warningVFX.SetWarning(false);
        if (animator != null) animator.SetTrigger("Attack");

        float remaining = dashLength;
        bool hasHit = false;
        while (remaining > 0f && !isDead)
        {
            float step = Mathf.Min(remaining, dashSpeed * Time.deltaTime);
            controller.Move(direction * step);
            remaining -= step;

            if (!hasHit && player != null && !player.isDead)
            {
                Vector3 offset = player.transform.position - transform.position;
                offset.y = 0f;
                if (offset.magnitude <= hitRadius)
                {
                    player.TakeDamage(dashDamage);
                    hasHit = true;
                }
            }
            yield return null;
        }
        if (lane != null) Destroy(lane.gameObject);
        yield return new WaitForSeconds(Mathf.Max(0f, recoverySeconds));
        busy = false;
    }
}
