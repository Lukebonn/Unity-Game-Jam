using UnityEngine;

/// <summary>Simple visible trigger projectile. Add to a small sphere prefab, or let EnemyRangedAI create one.</summary>
[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class EnemyProjectile : MonoBehaviour
{
    public float lifetime = 5f;
    private Vector3 direction;
    private float speed;
    private int damage;
    private bool initialized;

    private void Awake()
    {
        GetComponent<SphereCollider>().isTrigger = true;
        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    public void Initialize(Vector3 shotDirection, float shotSpeed, int shotDamage)
    {
        direction = shotDirection.normalized;
        speed = Mathf.Max(0.1f, shotSpeed);
        damage = Mathf.Max(1, shotDamage);
        initialized = true;
        Destroy(gameObject, Mathf.Max(0.5f, lifetime));
    }

    private void FixedUpdate()
    {
        if (!initialized) return;
        Rigidbody body = GetComponent<Rigidbody>();
        body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!initialized) return;
        if (other.GetComponentInParent<EnemyBase>() != null) return;
        //if (other.GetComponentInParent<RollableDie>() != null) return; // Enemy shots don't re-roll dice.

        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (!other.isTrigger) Destroy(gameObject); // environment
    }
}
