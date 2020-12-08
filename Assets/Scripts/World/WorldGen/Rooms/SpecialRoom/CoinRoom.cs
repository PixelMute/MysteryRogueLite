using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
class CoinRoom : SpecialRoom
{
    public CoinRoom() : base()
    {
        Info.MinHeight = 4;
        Info.MaxHeight = 4;
        Info.MinWidth = 4;
        Info.MaxWidth = 4;
    }

    public override void Decorate(Level level)
    {
        Vector2Int moneyLocation;
        if (ConnectionPoints.Count > 0)
        {
            var painter = new RoomPainter(level, this);
            var connection = ConnectionPoints.First().Value;
            if (connection.x == Bounds.Left)
            {
                level.Decorations.SetTile(new Vector3Int(Bounds.Left + 1, Bounds.Bottom + 1, 0), Painter.Bones);
                level.Decorations.SetTile(new Vector3Int(Bounds.Left + 1, Bounds.Bottom + 2, 0), Painter.Skull);
                painter.PlaceCobwebs(Bounds.Left + 2, Bounds.Bottom + 2, true);
                moneyLocation = new Vector2Int(Bounds.Left + 2, Bounds.Bottom + 1);
            }
            else if (connection.x == Bounds.Right)
            {
                level.Decorations.SetTile(new Vector3Int(Bounds.Left + 2, Bounds.Bottom + 1, 0), Painter.Bones);
                level.Decorations.SetTile(new Vector3Int(Bounds.Left + 2, Bounds.Bottom + 2, 0), Painter.Skull);
                painter.PlaceCobwebs(Bounds.Left + 1, Bounds.Bottom + 2, false);
                moneyLocation = new Vector2Int(Bounds.Left + 1, Bounds.Bottom + 1);
            }
            else if (connection.y == Bounds.Bottom)
            {
                level.Decorations.SetTile(new Vector3Int(Bounds.Left + 1, Bounds.Bottom + 1, 0), Painter.Bones);
                level.Decorations.SetTile(new Vector3Int(Bounds.Left + 2, Bounds.Bottom + 1, 0), Painter.Skull);
                painter.PlaceCobwebs(Bounds.Left + 2, Bounds.Bottom + 2, true);
                moneyLocation = new Vector2Int(Bounds.Left + 1, Bounds.Bottom + 2);
            }
            else
            {
                level.Decorations.SetTile(new Vector3Int(Bounds.Left + 1, Bounds.Bottom + 2, 0), Painter.Bones);
                level.Decorations.SetTile(new Vector3Int(Bounds.Left + 2, Bounds.Bottom + 2, 0), Painter.Skull);
                painter.PlaceCobwebs(Bounds.Left + 1, Bounds.Bottom + 1, false);
                moneyLocation = new Vector2Int(Bounds.Left + 2, Bounds.Bottom + 1);
            }
            BattleGrid.instance.SpawnMoneyOnTile(moneyLocation, 10);
        }
    }

    public override List<EnemyBody> GetAnyRequiredEnemies(Level level)
    {
        var possibleSpawnLocations = GetPossibleSpawnLocations(level);
        var spawnLoc = SeededRandom.PickRandom(possibleSpawnLocations);
        if (spawnLoc != null)
        {
            var brute = EnemySpawner.SpawnBrute(spawnLoc);
            return new List<EnemyBody> { brute.GetComponent<EnemyBody>() };
        }
        return new List<EnemyBody>();
    }
}

