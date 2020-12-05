using System.Collections.Generic;
using UnityEngine;

public class BossRoom : Room
{
    public static EnemyBody Boss;
    public BossRoom() : base(new RoomInfo()
    {
        MaxConnections = 2,
        MinWidth = 15,
        MaxWidth = 15,
        MinHeight = 15,
        MaxHeight = 15,
        RoomType = RoomType.BOSS,
    })
    { }

    public override List<Vector2Int> GetPossibleSpawnLocations(Level level)
    {
        //The boss room shouldn't have any random enemies spawn
        return new List<Vector2Int>();
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
            return true;
        }
        return false;
    }

    private Vector2Int GetBossSpawnLocation()
    {
        return new Vector2Int(Bounds.Left + Bounds.Width / 2, Bounds.Bottom + Bounds.Height / 2);
    }
}

