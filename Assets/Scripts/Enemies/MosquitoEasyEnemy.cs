public class MosquitoEasyEnemy : EnemyDashAI
{
    protected override void Reset()
    {
        base.Reset();
        maxHealth = 30; moveSpeed = 5.5f;
        comfortableDistance = 4f; dashTriggerRange = 7f;
        dashDamage = 12; windupSeconds = 0.45f; dashSpeed = 20f;
        maxDashDistance = 10f; hitRadius = 1.1f;
        recoverySeconds = 0.55f; dashCooldown = 2.2f;
    }
}
