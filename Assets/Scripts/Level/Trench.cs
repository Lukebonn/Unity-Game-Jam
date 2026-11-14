using UnityEngine;

// A hole in the floor. Scale X for its width and Z for its length, and only turn it in
// 90 degree steps: ArenaFloor cuts a box-shaped hole out of the ground where it is.
// The trigger box below the floor catches whatever falls in.
public class Trench : MonoBehaviour
{
    // The hole's outline on the ground: x is world X, y is world Z
    public Rect Footprint
    {
        get
        {
            Vector3 a = transform.TransformPoint(new Vector3(-0.5f, 0f, -0.5f));
            Vector3 b = transform.TransformPoint(new Vector3(0.5f, 0f, 0.5f));
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.z, b.z), Mathf.Max(a.x, b.x), Mathf.Max(a.z, b.z));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Enemies that fall in die (it still counts as a kill)
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(enemy.maxHealth);
            return;
        }

        // The player climbs back out where they fell in, minus some health
        PlayerCombat player = other.GetComponentInParent<PlayerCombat>();
        if (player != null)
            player.ClimbOutOfHole();
    }
}
