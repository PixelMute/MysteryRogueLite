using System.Collections;
using UnityEngine;

public class BossMeleeAttack : BasicMeleeAttack
{

    public override bool IsTargetInRange(Vector2Int target)
    {
        return IsTargetInRange(target, 2);
    }

    public override void ActivateAttack(Vector2Int target)
    {
        IsAttackDone = false;
        StartCoroutine(AttackCoroutine(target));
    }

    IEnumerator AttackCoroutine(Vector2Int target)
    {
        yield return new WaitForSeconds(.6f);
        base.ActivateAttack(target);
        IsAttackDone = true;
    }
}

