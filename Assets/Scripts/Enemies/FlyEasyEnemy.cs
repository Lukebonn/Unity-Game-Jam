public class FlyEasyEnemy : EnemyMeleeAI
{
    protected override void Reset()
    {
        base.Reset();
        maxHealth = 35; moveSpeed = 4.5f; stopDistance = 1.1f;
        attackStyle = AttackStyle.Contact; damagePerHit = 8;
        attackRange = 1.45f; hitArcDegrees = 360f;
        windupSeconds = 0.1f; recoverySeconds = 0.1f; attackCooldown = 0.9f;
    }
}
