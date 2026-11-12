public class MosquitoAssaultEnemy : EnemyRangedAI
{
    protected override void Reset()
    {
        base.Reset();
        maxHealth = 45; moveSpeed = 3.6f;
        minRange = 6f; idealRange = 9f; maxRange = 12f;
        damagePerProjectile = 6; projectileSpeed = 15f;
        bulletsPerBurst = 3; burstInterval = 0.16f;
        windupSeconds = 0.5f; fireCooldown = 2.2f; spreadDegrees = 5f;
    }
}
