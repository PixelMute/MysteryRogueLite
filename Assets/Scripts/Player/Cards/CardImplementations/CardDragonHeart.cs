using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CardDragonHeart : Card
{
    public CardDragonHeart()
    {
        cardName = "DragonHeart";
        cardDescription = "Gain 12 defense, then deal damage equal to defense in range 1-2. Does not need LOS.";
        energyCost = 2;
        spiritCost = 5;

        minRange = 1;
        maxRange = 2;
        conditions = new PlayCondition[] { PlayCondition.mustHitCreature };
        rangeCondition = GetRangeArray();
    }

    public override void CardPlayEffect(Vector2Int tileTarget)
    {
        BattleManager.player.ApplyStatusEffect(BattleManager.StatusEffectEnum.defense, 12);
        BattleGrid.instance.StrikeTile(tileTarget, BattleManager.player.GetStatusEffectValue(BattleManager.StatusEffectEnum.defense));
    }

    protected override bool[,] GetRangeArray()
    {
        bool[,] array = new bool[5, 5];
        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                if (!(i == 2 && j == 2))
                    array[i, j] = true;
            }
        }

        return array;
    }
}
