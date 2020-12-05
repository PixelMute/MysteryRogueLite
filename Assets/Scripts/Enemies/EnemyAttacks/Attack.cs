using System;
using UnityEngine;

public abstract class Attack : MonoBehaviour
{
    /// <summary>
    /// Returns true if the enemy can attack this target right now
    /// </summary>
    /// <param name="target">Target the enemy is attempting to attack</param>
    public abstract bool IsTargetInRange(Vector2Int target);

    protected bool IsTargetInRange(Vector2Int target, int range)
    {
        var xPos = transform.position.x;
        var zPos = transform.position.z;
        return Math.Abs(xPos - target.x) <= range && Math.Abs(zPos - target.y) <= range && BattleGrid.instance.CheckLoS(transform.position, BattleManager.ConvertVector(target, .05f));
    }

    /// <summary>
    /// Attacks the given target
    /// </summary>
    /// <param name="target">Target that we are attacking</param>
    public abstract void ActivateAttack(Vector2Int target);

    public bool IsAttackDone { get; protected set; } = true;
}

