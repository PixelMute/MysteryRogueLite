using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level
{
    public List<Room> Rooms { get; private set; }
    public Entrance Entrance { get; private set; }
    public Exit Exit { get; private set; }
    public Tilemap Terrain { get; private set; }
    public Tilemap Decorations { get; private set; }
    public bool HasBeenBuilt { get; private set; } = false;
    private RogueRect _bounds;
    private List<Vector2Int> _corners;
    public List<Vector2Int> Corners
    {
        get
        {
            if (_corners == null)
            {
                _corners = new List<Vector2Int>();
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
            if (_bounds == null)
            {
                _bounds = GetBoundary();
            }
            return _bounds;
        }
    }

    public Level(Tilemap terrain, Tilemap decorations)
    {
        Terrain = terrain;
        Decorations = decorations;
    }

    public void Build()
    {
        var rooms = InitRooms();
        var builder = new Builder();
        do
        {
            foreach (var room in rooms)
            {
                room.Reset();
            }
        } while (!builder.Build(rooms));
        Rooms = builder.PlacedRooms;
        CenterRooms();
        Paint();
        HasBeenBuilt = true;
    }

    public void Paint()
    {
        foreach (var room in Rooms)
        {
            room.Paint(this);
            room.PaintDoors(this);
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

    private List<Room> InitRooms()
    {
        var rooms = new List<Room>();
        Entrance = new Entrance();
        Exit = new Exit();
        rooms.Add(Entrance);
        rooms.Add(Exit);

        for (int i = 0; i < 4; i++)
        {
            rooms.Add(new StandardRoom());
        }
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
            for (int i = 0; i < room.ConnectionPoints.Count; i++)
            {
                room.ConnectionPoints[i] += new Vector2Int(-xOffset, -yOffset);
            }
        }
    }
}


