using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CardFootwork : Card
{
    public static GameObject wallPrefab;

    public CardFootwork()
    {
        cardName = "Footwork";
        cardDescription = "Move to an empty tile in range 1-1. Cuts corners.";
        energyCost = 1;
        spiritCost = 5;
        minRange = 1;
        maxRange = 1;
        conditions = new PlayCondition[] { PlayCondition.needsLOS, PlayCondition.emptyTile, PlayCondition.cornerCutting};
        rangeCondition = GetRangeArray();
    }

    public override void CardPlayEffect(Vector2Int tileTarget)
    {
        BattleManager.player.MoveTo(tileTarget, true);
    }

    protected override bool[,] GetRangeArray()
    {
        bool[,] array = new bool[,] { { true, true, true }, { true, false, true }, { true, true, true } };
        return array;
    }
}
