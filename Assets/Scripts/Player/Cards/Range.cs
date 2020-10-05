using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class RangeFactory
{
    public static List<PlayCondition> GetPlayConditionsFromString(string conditionString)
    {
        var split = conditionString.ToLower().Split(':');
        var playConditions = new List<PlayCondition>();
        for (int i = 0; i < split.Length; i++)
        {
            if (Enum.TryParse(split[i], true, out PlayCondition res))
            {
                playConditions.Add(res);
            }
        }
        return playConditions;
    }
}


public class Range
{
    public List<PlayCondition> PlayConditions { get; private set; }
    public int MinRange { get; private set; }
    public int MaxRange { get; private set; }
    public bool[,] RangeArray { get; private set; }

    public Range(List<PlayCondition> playConditions, int minRange, int maxRange)
    {
        PlayConditions = playConditions;
        MinRange = minRange;
        MaxRange = maxRange;
        RangeArray = GetRangeArray();
    }

    private bool[,] GetRangeArray()
    {
        var array = new bool[MaxRange * 2 + 1, MaxRange * 2 + 1];
        for (int i = 0; i < MaxRange * 2 + 1; i++)
        {
            for (int j = 0; j < MaxRange * 2 + 1; j++)
            {
                var distanceX = Math.Abs(i - MaxRange);
                var distanceY = Math.Abs(j - MaxRange);
                array[i, j] = distanceX <= MaxRange && distanceY <= MaxRange && (distanceX >= MinRange || distanceY >= MinRange);
            }
        }
        if (PlayConditions.Contains(PlayCondition.straightLine))
        {
            array = BattleManager.ANDArray(array, FillLineArray(MaxRange, true));
        }
        return array;
    }

    // Makes an array with lines through it.
    private bool[,] FillLineArray(int size, bool diagonal)
    {
        int arraySize = size * 2 + 1;
        bool[,] array = new bool[arraySize, arraySize];

        // Make something like
        // T X T X T
        // X T T T X
        // T T T T T
        // X T T T X
        // T X T X T
        for (int i = 0; i < arraySize; i++)
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

