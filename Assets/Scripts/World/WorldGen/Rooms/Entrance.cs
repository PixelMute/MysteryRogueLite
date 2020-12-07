using System.Collections.Generic;
using UnityEngine;

public class Entrance : StandardRoom
{
    private Vector2Int? _playerSpawn;
    public Vector2Int? PlayerSpawn
    {
        get
        {
            if (_playerSpawn == null)
            {
                _playerSpawn = FindPlayerSpawn();
            }
            return _playerSpawn;
        }
    }
    public Entrance() : base()
    {
        Info.RoomType = RoomType.ENTRANCE;
    }

    public override List<Vector2Int> GetPossibleSpawnLocations(Level level)
    {
        //Entrance should have no enemies that spawn
        return new List<Vector2Int>();
    }

    private Vector2Int? FindPlayerSpawn()
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
            return SeededRandom.PickRandom(possiblePoints);
        }
        return null;
    }
}

