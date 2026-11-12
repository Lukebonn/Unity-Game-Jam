public class FlyHardEnemy : EnemyMeleeAI
{
    protected override void Reset()
    {
        base.Reset();
        maxHealth = 160; moveSpeed = 2.8f; stopDistance = 2.9f;
        attackStyle = AttackStyle.HeavyFlip; damagePerHit = 28;
        attackRange = 3.2f; hitArcDegrees = 210f;
        windupSeconds = 1.0f; recoverySeconds = 0.8f; attackCooldown = 2f;
        leapDistance = 4.2f; leapDuration = 0.35f;
    }
}
