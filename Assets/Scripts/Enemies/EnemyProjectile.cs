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

    // The red lane on the ground showing this bullet's path (it fills up as the bullet flies)
    private GroundTelegraph lane;
    private float laneLength;
    private float travelled;

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

    // With a lane: the bullet lasts exactly as long as it takes to reach the end of the lane
    public void Initialize(Vector3 shotDirection, float shotSpeed, int shotDamage, GroundTelegraph warning, float reach)
    {
        lane = warning;
        laneLength = Mathf.Max(0.1f, reach);
        lifetime = Mathf.Min(lifetime, laneLength / Mathf.Max(0.1f, shotSpeed) + 0.05f);
        Initialize(shotDirection, shotSpeed, shotDamage);
    }

    private void FixedUpdate()
    {
        if (!initialized) return;
        Rigidbody body = GetComponent<Rigidbody>();
        body.MovePosition(body.position + direction * speed * Time.fixedDeltaTime);

        travelled += speed * Time.fixedDeltaTime;
        if (lane != null) lane.SetFill(travelled / laneLength);
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
