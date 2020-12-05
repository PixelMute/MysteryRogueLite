using System.Collections.Generic;
using UnityEngine;

class BossRoom : Room
{
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
        var boss = EnemySpawner.SpawnBoss(GetBossSpawnLocation());
        return new List<EnemyBody> { boss.GetComponent<EnemyBody>() };
    }

    private Vector2Int GetBossSpawnLocation()
    {
        return new Vector2Int(Bounds.Left + Bounds.Width / 2, Bounds.Bottom + Bounds.Height / 2);
    }
}

