using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CardCinders : Card
{
    float damage = 10f;
    public CardCinders()
    {
        cardName = "Cinders";
        cardDescription = "Deal 10 damage.\nRange 4. Line.";
        energyCost = 1;
        spiritCost = 4;
        minRange = 1;
        maxRange = 4;
        conditions = new PlayCondition[] { PlayCondition.needsLOS, PlayCondition.straightLine, PlayCondition.mustHitCreature };
        rangeCondition = GetRangeArray();
    }

    public override void CardPlayEffect(Vector2Int tileTarget)
    {
        BattleGrid.instance.StrikeTile(tileTarget, damage);
    }

    protected override bool[,] GetRangeArray()
    {
        bool[,] array = Card.FillLineArray(4, true);

        array[4, 4] = false;

        return array;
    }
}
