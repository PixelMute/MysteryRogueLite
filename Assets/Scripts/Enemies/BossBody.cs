class BossBody : EnemyBody
{
    public BossMeleeAttack Melee { get; set; }
    public new BossAnimation Animation
    {
        get
        {
            return base.Animation as BossAnimation;
        }
        set
        {
            base.Animation = value;
        }
    }

    public override void Awake()
    {
        base.Awake();
        Melee = GetComponent<BossMeleeAttack>();
    }
}

