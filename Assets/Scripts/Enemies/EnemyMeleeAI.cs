using System.Collections;
using UnityEngine;

/// <summary>Shared chase + telegraphed melee logic for the four Fly archetypes.</summary>
[RequireComponent(typeof(CharacterController))]
public abstract class EnemyMeleeAI : EnemyBase
{
    public enum AttackStyle { Contact, Flail, Axe, HeavyFlip }

    [Header("Melee Movement")]
    public float moveSpeed = 3.5f;
    public float rotationSpeed = 12f;
    public float stopDistance = 1.2f;

    [Header("Melee Attack")]
    public AttackStyle attackStyle;
    public int damagePerHit = 10;
    public float attackRange = 1.7f;
    [Range(0f, 360f)] public float hitArcDegrees = 180f;
    public float windupSeconds = 0.4f;
    public float recoverySeconds = 0.35f;
    public float attackCooldown = 0.8f;
    [Tooltip("HeavyFlip only: distance traveled during the flip.")]
    public float leapDistance = 4f;
    public float leapDuration = 0.3f;

    [Header("Optional Animation")]
    public Animator animator;
    [Tooltip("For HeavyFlip, assign the model pivot, not the CharacterController root.")]
    public Transform visualRoot;

    private CharacterController controller;
    private EnemyTelegraphVFX warningVFX;
    private bool attacking;
    private bool leaping;
    private float nextAttackTime;

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
        if (isDead || player == null || player.isDead) return;
        if (leaping) return;

        Vector3 direction = player.transform.position - transform.position;
        direction.y = 0f;
        float distance = direction.magnitude;

        if (attacking)
        {
            controller.SimpleMove(Vector3.zero);
            return;
        }

        // Follow the NavMesh path so pillars get walked around, and face the way we're walking
        Vector3 move = distance > stopDistance ? PathDirectionTo(player.transform.position) : Vector3.zero;
        Vector3 face = move.sqrMagnitude > 0.01f ? move : direction;
        if (face.sqrMagnitude > 0.0025f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(face), Time.deltaTime * rotationSpeed);

        controller.SimpleMove(move * moveSpeed);
        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + Mathf.Max(0.1f, attackCooldown);
            StartCoroutine(AttackRoutine());
        }
    }

    private IEnumerator AttackRoutine()
    {
        attacking = true;
        if (animator != null) animator.SetTrigger("Windup");
        if (warningVFX != null) warningVFX.SetWarning(true);

        // Capture target position at the START of the windup so a heavy attack can be dodged.
        Vector3 targetAtWindup = player != null ? player.transform.position : transform.position;
        yield return new WaitForSeconds(Mathf.Max(0f, windupSeconds));

        if (isDead || player == null || player.isDead)
        {
            if (warningVFX != null) warningVFX.SetWarning(false);
            attacking = false;
            yield break;
        }

        if (warningVFX != null) warningVFX.SetWarning(false);
        if (animator != null) animator.SetTrigger("Attack");
        if (attackStyle == AttackStyle.HeavyFlip)
        {
            Vector3 leapDirection = targetAtWindup - transform.position;
            leapDirection.y = 0f;
            leapDirection = leapDirection.sqrMagnitude < 0.01f ? transform.forward : leapDirection.normalized;
            transform.rotation = Quaternion.LookRotation(leapDirection);

            Quaternion originalRotation = visualRoot != null ? visualRoot.localRotation : Quaternion.identity;
            float duration = Mathf.Max(0.05f, leapDuration);
            float travelDistance = Mathf.Min(leapDistance, Mathf.Max(0f, Vector3.Distance(
                new Vector3(transform.position.x, 0f, transform.position.z),
                new Vector3(targetAtWindup.x, 0f, targetAtWindup.z)) - 0.7f));
            float elapsed = 0f;
            leaping = true;
            while (elapsed < duration && !isDead)
            {
                float step = Mathf.Min(Time.deltaTime, duration - elapsed);
                controller.Move(leapDirection * (travelDistance / duration) * step);
                if (visualRoot != null)
                    visualRoot.localRotation = originalRotation *
                        Quaternion.Euler(360f * (elapsed + step) / duration, 0f, 0f);
                elapsed += step;
                yield return null;
            }
            if (visualRoot != null) visualRoot.localRotation = originalRotation;
            leaping = false;
        }

        DealMeleeDamage();
        yield return new WaitForSeconds(Mathf.Max(0f, recoverySeconds));
        attacking = false;
    }

    private void DealMeleeDamage()
    {
        if (player == null || player.isDead) return;
        Vector3 toPlayer = player.transform.position - transform.position;
        toPlayer.y = 0f;
        if (toPlayer.magnitude > attackRange + 0.4f) return;
        if (toPlayer.sqrMagnitude > 0.01f &&
            Vector3.Angle(transform.forward, toPlayer) > hitArcDegrees * 0.5f) return;

        player.TakeDamage(damagePerHit);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
