﻿using System;
using UnityEngine;

class BasicRangedAttack : Attack
{
    public int MinDamage;
    public int MaxDamage;
    public GameObject Arrow;

    public override bool IsTargetInRange(Vector2Int target)
    {
        var xPos = transform.position.x;
        var zPos = transform.position.z;
        return Math.Abs(xPos - target.x) < 3 && Math.Abs(zPos - target.y) < 3;
    }

    public override void ActivateAttack(Vector2Int target)
    {
        var arrow = Instantiate(Arrow, transform.position, Quaternion.Euler(new Vector3(90, 0, 0))).GetComponent<Arrow>();
        arrow.ShootAtTarget(target, AttackTarget);
    }

    private void AttackTarget(Vector2Int target)
    {
        var random = new System.Random();
        var damage = random.Next(MinDamage, MaxDamage + 1);
        BattleGrid.instance.StrikeTile(target, BattleManager.ConvertVector(transform.position), damage);
    }
}

