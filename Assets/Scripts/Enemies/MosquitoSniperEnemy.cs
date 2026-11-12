public class MosquitoSniperEnemy : EnemyRangedAI
{
    protected override void Reset()
    {
        base.Reset();
        maxHealth = 42; moveSpeed = 2.5f;
        minRange = 9f; idealRange = 15f; maxRange = 21f;
        damagePerProjectile = 24; projectileSpeed = 26f;
        bulletsPerBurst = 1; windupSeconds = 1.25f;
        fireCooldown = 3.0f; spreadDegrees = 0f;
    }
}
