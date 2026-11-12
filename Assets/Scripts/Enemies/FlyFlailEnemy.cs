public class FlyFlailEnemy : EnemyMeleeAI
{
    protected override void Reset()
    {
        base.Reset();
        maxHealth = 80; moveSpeed = 3f; stopDistance = 2.1f;
        attackStyle = AttackStyle.Flail; damagePerHit = 13;
        attackRange = 2.8f; hitArcDegrees = 170f;
        windupSeconds = 0.65f; recoverySeconds = 0.45f; attackCooldown = 1.3f;
    }
}
