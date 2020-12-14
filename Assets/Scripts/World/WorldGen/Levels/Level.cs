using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class Level
{
    public List<Room> Rooms { get; protected set; }
    public Entrance Entrance { get; protected set; }
    public Exit Exit { get; protected set; }
    [field: NonSerialized]
    public Tilemap Terrain { get; set; }
    [field: NonSerialized]
    public Tilemap Decorations { get; set; }
    public bool HasBeenBuilt { get; protected set; } = false;
    private RogueRect _bounds;
    private List<SerializableVector2Int> _corners;
    public List<SerializableVector2Int> Corners
    {
        get
        {
            if (_corners == null)
            {
                _corners = new List<SerializableVector2Int>();
                foreach (var room in Rooms)
                {
                    _corners.AddRange(room.Corners);
                }
            }
            return _corners;
        }
    }


    public RogueRect Bounds
    {
        get
        {
            return GetBoundary();
        }
    }

    public Level(Tilemap terrain, DecorativeTileMap decorativeTileMap)
    {
        Terrain = terrain;
        Decorations = decorativeTileMap.TileMap;
    }

    public void Build()
    {
        var rooms = InitRooms();
        var builder = GetBuilder();
        int count = 0;
        do
        {
            foreach (var room in rooms)
            {
                room.Reset();
            }
            count++;
        } while (!builder.Build(rooms));
        Debug.Log($"Took {count} attempts to correctly build the level");
        Rooms = builder.PlacedRooms;
        CenterRooms();
        HasBeenBuilt = true;

        //DebugRooms();
    }

    protected virtual Builder GetBuilder()
    {
        var res = new LoopBuilder();
        return res;
    }

    private void DebugRooms()
    {
        foreach (var room in Rooms)
        {
            var rng = UnityEngine.Random.ColorHSV();
            Helpers.DrawDebugLine(room.Bounds.Left, room.Bounds.Bottom, rng);
            Helpers.DrawDebugLine(room.Bounds.Right, room.Bounds.Bottom, rng);
            Helpers.DrawDebugLine(room.Bounds.Left, room.Bounds.Top, rng);
            Helpers.DrawDebugLine(room.Bounds.Right, room.Bounds.Top, rng);
        }
    }

    public void Paint()
    {
        foreach (var room in Rooms)
        {
            room.Paint(this);
            room.PaintDoors(this);
            room.Decorate(this);
        }
        var stairsLoc = Exit.StairsLocation;
        if (stairsLoc != null)
        {
            Decorations.SetTile(new Vector3Int(stairsLoc.Value.x, stairsLoc.Value.y, 0), Painter.LadderDown);
        }
        else
        {
            Debug.LogError("Exit doesn't have valid space for stairs down. This is very bad");
        }
    }

    public void ClearAllTiles()
    {
        Terrain.ClearAllTiles();
        Decorations.ClearAllTiles();
    }

    public virtual List<Vector2Int> GetPossibleSpawnLocations()
    {
        var res = new List<Vector2Int>();
        foreach (var room in Rooms)
        {
            var roomLocations = room.GetPossibleSpawnLocations(this);
            if (roomLocations != null)
            {
                res.AddRange(roomLocations);
            }
        }
        return res;
    }

    public virtual List<EnemyBody> GetRequiredEnemies()
    {
        var res = new List<EnemyBody>();
        foreach (var room in Rooms)
        {
            var enemies = room.GetAnyRequiredEnemies(this);
            if (enemies != null)
            {
                res.AddRange(enemies);
            }
        }
        return res;
    }

    public List<Trap> GetRequiredTraps()
    {
        var res = new List<Trap>();
        foreach (var room in Rooms)
        {
            res.AddRange(room.GetAnyRequiredTraps(this));
        }
        return res;
    }

    public List<TreasureChest> GetRequiredTreasure()
    {
        var res = new List<TreasureChest>();
        foreach (var room in Rooms)
        {
            res.AddRange(room.GetAnyRequiredTreasure(this));
        }
        return res;
    }

    private RogueRect GetBoundary()
    {
        var bounds = new RogueRect()
        {
            Left = int.MaxValue,
            Right = int.MinValue,
            Top = int.MinValue,
            Bottom = int.MaxValue,
        };
        foreach (var room in Rooms)
        {
            if (room.Bounds.Left < bounds.Left)
            {
                bounds.Left = room.Bounds.Left;
            }
            if (room.Bounds.Right > bounds.Right)
            {
                bounds.Right = room.Bounds.Right;
            }
            if (room.Bounds.Top > bounds.Top)
            {
                bounds.Top = room.Bounds.Top;
            }
            if (room.Bounds.Bottom < bounds.Bottom)
            {
                bounds.Bottom = room.Bounds.Bottom;
            }
        }
        return bounds;
    }

    protected virtual List<Room> InitRooms()
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

    /// <summary>
    /// Sets the rooms so the bottom left of the level is at (0,0)
    /// </summary>
    private void CenterRooms()
    {
        var xOffset = Bounds.Left;
        var yOffset = Bounds.Bottom;
        foreach (var room in Rooms)
        {
            room.Bounds.Shift(-xOffset, -yOffset);
            foreach (var neighbor in room.Neighbors)
            {
                room.ConnectionPoints[neighbor] += new Vector2Int(-xOffset, -yOffset);
            }
        }
    }
}


