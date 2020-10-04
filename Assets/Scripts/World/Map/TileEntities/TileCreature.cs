using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A subclass of TileEntity that represents living, moving entities on the battlegrid.
public abstract class TileCreature : TileEntity
{
    public int visionRange = 5;
    protected Dictionary<BattleManager.StatusEffectEnum, StatusEffectDataHolder> statusEffects = null; // Dictionary that stores what status effects are currently on this entity.

    public abstract override float GetPathfindingCost();
    public abstract override bool GetPlayerWalkable();
    public abstract override void TakeDamage(int amount);

    public abstract int GetStatusEffectValue(BattleManager.StatusEffectEnum status);

    public abstract void ApplyStatusEffect(BattleManager.StatusEffectEnum status, int amount);
}
