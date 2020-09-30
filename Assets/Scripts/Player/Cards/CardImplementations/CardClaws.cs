using System.Collections;
using System.Collections.Generic;
using Roguelike;
using UnityEngine;

public class CardClaws : Card
{
    private float damage = 20f;
    public CardClaws()
    {
        cardName = "Claws";
        cardDescription = "Deal 20 damage.\nRange1.";
        energyCost = 1;
        spiritCost = 3;

        minRange = 1;
        maxRange = 1;
        conditions = new PlayCondition[] { PlayCondition.needsLOS, PlayCondition.mustHitCreature};
        rangeCondition = GetRangeArray();
    }

    public override void CardPlayEffect(Vector2Int tileTarget)
    {
        // For now, just spawn a particle
        GlobalUIManager.instance.SpawnParticleAt(GlobalUIManager.ParticleType.sword, BattleManager.ConvertVector(tileTarget, 0.9f));
        BattleGrid.instance.StrikeTile(tileTarget, damage);
    }

    protected override bool[,] GetRangeArray()
    {
        bool[,] array = new bool[,] { { true, true, true }, { true, false, true }, { true, true, true } };
        return array;
    }
}
