using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class BossLevel : Level
{
    public BossLevel(Tilemap terrain, DecorativeTileMap decorativeTileMap) : base(terrain, decorativeTileMap) { }

    protected override Builder GetBuilder()
    {
        var res = new BossLevelBuilder
        {
            BossRoom = new BossRoom()
        };
        return res;
    }

    protected override List<Room> InitRooms()
    {
        var rooms = new List<Room>();
        Entrance = new Entrance();
        Exit = new Exit();
        rooms.Add(Entrance);
        rooms.Add(Exit);

        for (int i = 0; i < 1; i++)
        {
            rooms.Add(new StandardRoom());
        }
        rooms.Add(new LargeSqaureRoom());
        rooms.Add(new CoinRoom());
        //for (int i = 0; i < 2; i++)
        //{
        //    rooms.Add(new SpecialRoom());
        //}
        return rooms;
    }
}

