public class MosquitoHardEnemy : EnemyRangedAI
{
    protected override void Reset()
    {
        base.Reset();
        maxHealth = 100; moveSpeed = 4.3f;
        minRange = 2.8f; idealRange = 5.5f; maxRange = 8f;
        damagePerProjectile = 4; projectileSpeed = 19f;
        bulletsPerBurst = 9; burstInterval = 0.09f;
        windupSeconds = 0.28f; fireCooldown = 2.1f; spreadDegrees = 11f;
    }
}
