public class FlyAxeEnemy : EnemyMeleeAI
{
    protected override void Reset()
    {
        base.Reset();
        maxHealth = 90; moveSpeed = 3.4f; stopDistance = 1.5f;
        attackStyle = AttackStyle.Axe; damagePerHit = 18;
        attackRange = 2.25f; hitArcDegrees = 110f;
        windupSeconds = 0.6f; recoverySeconds = 0.5f; attackCooldown = 1.4f;
    }
}
