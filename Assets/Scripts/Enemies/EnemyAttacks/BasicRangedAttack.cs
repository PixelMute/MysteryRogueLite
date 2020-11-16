using System;
using UnityEngine;

class BasicRangedAttack : Attack
{
    public int MinDamage;
    public int MaxDamage;

    public override bool IsTargetInRange(Vector2Int target)
    {
        var xPos = transform.position.x;
        var zPos = transform.position.z;
        return Math.Abs(xPos - target.x) < 3 && Math.Abs(zPos - target.y) < 3;
    }

    public override void ActivateAttack(Vector2Int target)
    {
        var random = new System.Random();
        var damage = random.Next(MinDamage, MaxDamage + 1);
        BattleGrid.instance.StrikeTile(target, damage);
    }
}

