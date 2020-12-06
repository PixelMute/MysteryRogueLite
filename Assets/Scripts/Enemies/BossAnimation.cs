using UnityEngine;

class BossAnimation : EnemyAnimation
{
    public GameObject MinionParticleSystem;
    private ParticleSystem minionEffect;

    public void Update()
    {
        if (minionEffect != null)
        {
            if (!minionEffect.isPlaying)
            {
                Destroy(minionEffect.gameObject);
            }
        }
    }

    public override void Move()
    {
        Animator.Play("Idle");
    }

    public void Melee()
    {
        Animator.Play("Melee");
    }

    public void Glowing()
    {
        Animator.Play("Glowing");
    }

    public void BecomeInvincible()
    {
        Animator.Play("InToRock");
    }

    public void BecomeVincible()
    {
        Animator.Play("OutOfRock");
    }

    public void LaserCast()
    {
        Animator.Play("LaserCast");
    }

    public void MinionSpawnEffect(Vector2Int location)
    {
        minionEffect = Instantiate(MinionParticleSystem, BattleManager.ConvertVector(location, 0), Quaternion.identity).GetComponent<ParticleSystem>();
        minionEffect.Play();
    }
}

