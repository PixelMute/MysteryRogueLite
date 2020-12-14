using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class BossLevel : Level
{
    public BossRoom BossRoom;
    public BossLevel(Tilemap terrain, DecorativeTileMap decorativeTileMap) : base(terrain, decorativeTileMap) { }

    protected override Builder GetBuilder()
    {
        var res = new BossLevelBuilder
        {
            BossRoom = new BossRoom()
        };
        BossRoom = res.BossRoom;
        return res;
    }

    protected override List<Room> InitRooms()
    {
        var rooms = new List<Room>();
        Entrance = new Entrance();
        Exit = new Exit();
        rooms.Add(Entrance);
        rooms.Add(Exit);

        for (int i = 0; i < CustomRun.instance.RoomsPerFloor - 2; i++)
        {
            rooms.Add(new StandardRoom());
        }
        var randomInt = SeededRandom.Range(0, rooms.Count);
        rooms.Insert(randomInt, new LargeSqaureRoom());
        rooms.Add(new CoinRoom());
        //for (int i = 0; i < 2; i++)
        //{
        //    rooms.Add(new SpecialRoom());
        //}
        return rooms;
    }

    public override List<Vector2Int> GetPossibleSpawnLocations()
    {
        var res = new List<Vector2Int>();
        foreach (var room in Rooms)
        {
            if (room != Exit)   //Don't spawn enemies in the exit room on boss levels
            {
                var roomLocations = room.GetPossibleSpawnLocations(this);
                if (roomLocations != null)
                {
                    res.AddRange(roomLocations);
                }
            }
        }
        return res;
    }

    public void LockBossRoom()
    {

    }
}

