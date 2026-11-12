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
        if (distance > 0.05f) transform.rotation = Quaternion.LookRotation(toward);

        controller.SimpleMove(distance > comfortableDistance ? toward.normalized * moveSpeed : Vector3.zero);
        if (distance <= dashTriggerRange && Time.time >= nextDashTime)
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
        yield return new WaitForSeconds(Mathf.Max(0f, windupSeconds));
        if (isDead || player == null || player.isDead)
        {
            if (warningVFX != null) warningVFX.SetWarning(false);
            busy = false;
            yield break;
        }

        Vector3 travel = target - transform.position;
        travel.y = 0f;
        Vector3 direction = travel.sqrMagnitude > 0.01f ? travel.normalized : transform.forward;
        transform.rotation = Quaternion.LookRotation(direction);
        if (warningVFX != null) warningVFX.SetWarning(false);
        if (animator != null) animator.SetTrigger("Attack");

        float remaining = Mathf.Min(maxDashDistance, travel.magnitude + 1f);
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
        yield return new WaitForSeconds(Mathf.Max(0f, recoverySeconds));
        busy = false;
    }
}
