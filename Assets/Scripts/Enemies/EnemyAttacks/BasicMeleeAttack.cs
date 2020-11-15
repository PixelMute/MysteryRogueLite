using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class BasicMeleeAttack : Attack
{
    public int MaxDamage { get; set; }
    public int MinDamage { get; set; }

    public override bool IsTargetInRange(Vector2Int target)
    {
        return Vector2Int.Distance(target, BattleManager.ConvertVector(transform.position)) < 2;
    }

    public override void ActivateAttack(Vector2Int target)
    {
        var random = new System.Random();
        var damage = random.Next(MinDamage, MaxDamage + 1);
        BattleGrid.instance.StrikeTile(target, damage);
    }
}
