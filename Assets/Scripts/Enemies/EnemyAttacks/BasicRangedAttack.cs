using UnityEngine;

class BasicRangedAttack : Attack
{
    public int MinDamage;
    public int MaxDamage;
    public int Range = 3;
    public GameObject Arrow;
    private Arrow _arrow;

    public void Update()
    {
        if (_arrow == null)
        {
            IsAttackDone = true;
        }
    }

    public override bool IsTargetInRange(Vector2Int target)
    {
        return IsTargetInRange(target, Range);
    }

    public override void ActivateAttack(Vector2Int target)
    {
        IsAttackDone = false;
        _arrow = Instantiate(Arrow, transform.position, Quaternion.Euler(new Vector3(90, 0, 0))).GetComponent<Arrow>();
        _arrow.ShootAtTarget(target, AttackTarget);
    }

    private void AttackTarget(Vector2Int target)
    {
        var random = new System.Random();
        var damage = random.Next(MinDamage, MaxDamage + 1);
        BattleGrid.instance.StrikeTile(target, BattleManager.ConvertVector(transform.position), damage);
    }
}

