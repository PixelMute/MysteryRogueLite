using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class RangeFactory
{
    public static Range GetRangeFromString(string rangeString)
    {
        var split = rangeString.ToLower().Split(':');
        var playConditions = new List<PlayCondition>();
        for (int i = 1; i < split.Length; i++)
        {
            if (Enum.TryParse(split[i], true, out PlayCondition res))
            {
                playConditions.Add(res);
            }
        }
        switch (split[0])
        {
            case "melee":
                return new MeleeRange(playConditions);
        }
        return null;
    }
}


public abstract class Range
{
    public List<PlayCondition> PlayConditions { get; protected set; }

    public Range(List<PlayCondition> playConditions)
    {
        PlayConditions = playConditions;
    }

    /// <summary>
    /// Returns whether or not the target is within range of the player.
    /// Assumes that all play conditions have been met for this range
    /// </summary>
    /// <param name="player">Location of entity playing the card</param>
    /// <param name="target">Location of target</param>
    /// <returns></returns>
    public abstract bool IsTargetInRange(Vector2Int player, Vector2Int target);

    // Sets up the array of possible target tiles
    public abstract bool[,] GetRangeArray();
}

public class MeleeRange : Range
{
    public MeleeRange(List<PlayCondition> playConditions) : base(playConditions) { }

    public override bool IsTargetInRange(Vector2Int player, Vector2Int target)
    {
        var xDiff = Math.Abs(player.x - target.x);
        var yDiff = Math.Abs(player.y - target.y);
        return xDiff == 1 || yDiff == 1;
    }

    public override bool[,] GetRangeArray()
    {
        bool[,] array = new bool[,] { { true, true, true }, { true, false, true }, { true, true, true } };
        return array;
    }
}


