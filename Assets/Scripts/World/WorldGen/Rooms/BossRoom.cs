using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BossRoom : Room
{
    public static EnemyBody Boss;
    public BossRoom() : base(new RoomInfo()
    {
        MaxConnections = 2,
        MinWidth = 13,
        MaxWidth = 13,
        MinHeight = 13,
        MaxHeight = 13,
        RoomType = RoomType.BOSS,
    })
    { }

    public override List<Vector2Int> GetPossibleSpawnLocations(Level level)
    {
        //The boss room shouldn't have any random enemies spawn
        return new List<Vector2Int>();
    }

    public List<Vector2Int> GetPossibleSpawnLocations()
    {
        var rect = new RogueRect()
        {
            Left = Bounds.Left + 1,
            Right = Bounds.Right - 2,
            Bottom = Bounds.Bottom + 1,
            Top = Bounds.Top - 2
        };
        return rect.GetPoints();
    }

    public override List<EnemyBody> GetAnyRequiredEnemies(Level level)
    {
        var spawnLoc = GetBossSpawnLocation();
        Boss = EnemySpawner.SpawnBoss(spawnLoc).GetComponent<EnemyBody>();
        Boss.xPos = spawnLoc.x;
        Boss.zPos = spawnLoc.y;
        (Boss.AI as BossBrain).BossRoom = this;
        return new List<EnemyBody> { Boss.GetComponent<EnemyBody>() };
    }

    public bool ActivateBoss()
    {
        var x = BattleManager.player.xPos;
        var z = BattleManager.player.zPos;
        if (x >= Bounds.Left + 1 && x <= Bounds.Right - 2 && z <= Bounds.Top - 2 && z >= Bounds.Bottom + 1)
        {
            Debug.Log("Activating boss");
            AudioManager.PlayBossMusic();

            var level = BattleGrid.instance.CurrentFloor.Level as BossLevel;
            if (level != null)
            {
                base.Paint(level);
                foreach (var connection in ConnectionPoints.Values)
                {
                    var i = connection.x == Bounds.Right ? connection.x - 1 : connection.x;
                    var j = connection.y == Bounds.Top ? connection.y - 1 : connection.y;
                    BattleGrid.instance.CurrentFloor.SpawnWallAt(i, j);
                }
            }

            return true;
        }
        return false;
    }

    public void UnlockRoom()
    {
        AudioManager.PlayBackgroundMusic();
        var level = BattleGrid.instance.CurrentFloor.Level as BossLevel;
        if (level != null)
        {
            base.Paint(level);
            base.PaintDoors(level);
            foreach (var connection in ConnectionPoints.Values)
            {
                var x = connection.x == Bounds.Right ? connection.x - 1 : connection.x;
                var y = connection.y == Bounds.Top ? connection.y - 1 : connection.y;
                BattleGrid.instance.CurrentFloor.DestroyWallAt(x, y);
            }
        }
    }

    public bool IsInsideRoom(Vector2Int location)
    {
        return location.x >= Bounds.Left && location.x <= Bounds.Right - 1 && location.y >= Bounds.Bottom && location.y <= Bounds.Top - 1;
    }

    public bool IsEntityAllowedIn(TileEntity entity)
    {
        //Only entities allowed are boss, minion and player
        if (entity is BossBody)
        {
            return true;
        }
        if (entity is EnemyBody)
        {
            if (((EnemyBody)entity).AI is MinionBrain)
            {
                return true;
            }
        }
        return entity is PlayerController;
    }

    private Vector2Int GetBossSpawnLocation()
    {
        return new Vector2Int(Bounds.Left + Bounds.Width / 2, Bounds.Bottom + Bounds.Height / 2);
    }


}

