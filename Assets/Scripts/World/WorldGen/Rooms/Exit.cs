using System.Collections.Generic;
using UnityEngine;

public class Exit : StandardRoom
{
    private Vector2Int? _stairsLocation;
    public Vector2Int? StairsLocation
    {
        get
        {
            if (_stairsLocation == null)
            {
                _stairsLocation = FindStairsLocation();
            }
            return _stairsLocation;
        }
    }
    public Exit() : base()
    {
        Info.RoomType = RoomType.EXIT;
    }

    public override List<Vector2Int> GetPossibleSpawnLocations(Level level)
    {
        var baseLocations = base.GetPossibleSpawnLocations(level);
        if (StairsLocation != null)
        {
            //Don't spawn any enemies on the exit stairs
            baseLocations.Remove(StairsLocation.Value);
        }
        return baseLocations;
    }

    private Vector2Int? FindStairsLocation()
    {
        var spawnArea = new RogueRect()
        {
            Left = Bounds.Left + 2,
            Right = Bounds.Right - 3,
            Top = Bounds.Top - 3,
            Bottom = Bounds.Bottom + 2
        };
        var possiblePoints = spawnArea.GetPoints();
        if (possiblePoints.Count > 0)
        {
            return possiblePoints.PickRandom();
        }
        return null;
    }
}

