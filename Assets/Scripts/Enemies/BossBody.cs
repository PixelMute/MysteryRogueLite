using UnityEngine;

class BossBody : EnemyBody
{
    public BossMeleeAttack Melee { get; set; }
    public bool Invincible { get; set; } = false;
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

    public new BossBrain AI
    {
        get
        {
            return base.AI as BossBrain;
        }
    }

    public override void Awake()
    {
        base.Awake();
        Melee = GetComponent<BossMeleeAttack>();
    }

    public override int TakeDamage(Vector2Int locationOfAttack, int amount)
    {
        if (!Invincible)
        {
            var amountTaken = base.TakeDamage(locationOfAttack, amount);
            if (Health.CurrentHealth <= Health.MaxHealth / 2 && !AI.HasBeenInvincible)
            {
                AI.BecomeInvicible();
            }
        }
        return 0;
    }
}

