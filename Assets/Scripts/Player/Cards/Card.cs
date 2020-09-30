
// This class stores data related to a single card. Goes on the cardManager.
using UnityEngine;
using System.Collections.Generic;

public abstract class Card
{
    // This enum describes different conditions for playing the card.
    public enum PlayCondition { needsLOS, straightLine, emptyTile, cornerCutting, mustHitCreature };
    public PlayCondition[] conditions;
    public bool[,] rangeCondition; // Which tiles this card can hit, relative to the player.

    public int maxRange = 1;
    public int minRange = 1;

    public string cardName;
    public string cardDescription;
    public int energyCost;
    public int spiritCost = 5;

    public TileCreature owner;
    // TODO: Support for upgrades

    public override string ToString()
    {
        return cardName;
    }

    // Sets up the array of possible target tiles
    protected abstract bool[,] GetRangeArray();

    public abstract void CardPlayEffect(Vector2Int tileTarget);

    // Makes an array with lines through it.
    public static bool[,] FillLineArray(int size, bool diagonal)
    {
        int arraySize = size * 2 + 1;
        bool[,] array = new bool[arraySize,arraySize];

        // Make something like
        // T X T X T
        // X T T T X
        // T T T T T
        // X T T T X
        // T X T X T
        for (int i = 0; i < arraySize; i ++)
        {
            array[i, size] = true;
            array[size, i] = true;
            if (diagonal)
            {
                array[i, i] = true;
                array[arraySize - i - 1, i] = true;
            }
            
        }

        return array;
    }
}