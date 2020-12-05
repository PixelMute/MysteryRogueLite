class BossAnimation : EnemyAnimation
{
    public override void Move()
    {
        Animator.Play("Idle");
    }

    public void Melee()
    {
        Animator.Play("Melee");
    }
}

