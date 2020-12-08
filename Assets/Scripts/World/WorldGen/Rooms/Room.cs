using System;
using System.Collections.Generic;
using UnityEngine;

public enum DiRogueRection
{
    LEFT,
    RIGHT,
    UP,
    DOWN,
    ALL
}

public enum RoomType
{
    STANDARD,
    ENTRANCE,
    EXIT,
    CONNECTION,
    SPECIAL,
    BOSS
}

[Serializable]
public class RoomInfo
{
    public int MaxWidth { get; set; }
    public int MinWidth { get; set; }
    public int MaxHeight { get; set; }
    public int MinHeight { get; set; }
    public int MaxConnections { get; set; }
    public int MinConnections { get; set; } = 1;        //Assume every room must have atleast one connection
    public RoomType RoomType { get; set; }
}

[Serializable]
public abstract class Room
{
    public RogueRect Bounds { get; private set; }
    public RoomInfo Info { get; private set; }

    public List<Room> Neighbors { get; private set; }

    public Dictionary<Room, SerializableVector2Int> ConnectionPoints { get; private set; }
    public List<SerializableVector2Int> Corners { get; private set; }

    public Room(RoomInfo info)
    {
        Info = info;
        Bounds = new RogueRect();
        Neighbors = new List<Room>();
        ConnectionPoints = new Dictionary<Room, SerializableVector2Int>();
        Corners = new List<SerializableVector2Int>();
    }

    public void Reset()
    {
        Bounds = new RogueRect();
        ConnectionPoints = new Dictionary<Room, SerializableVector2Int>();
        Corners = new List<SerializableVector2Int>();
        foreach (var room in Neighbors)
        {
            room.Neighbors.Remove(this);
            room.ConnectionPoints.Remove(this);
        }
        Neighbors = new List<Room>();
    }

    public void SetSize()
    {
        var width = SeededRandom.Range(Info.MinWidth, Info.MaxWidth);
        var height = SeededRandom.Range(Info.MinHeight, Info.MaxHeight);
        Bounds.Resize(width, height);
    }

    public bool SetSizeWithLimit(int width, int height)
    {
        if (width < Info.MinWidth || height < Info.MinHeight)
        {
            return false;
        }
        //Set random size of the room
        SetSize();
        //If room is larger than the limit, resize the room
        if (Bounds.Width > width || Bounds.Height > height)
        {
            Bounds.Resize(Math.Min(Bounds.Width, width), Math.Min(Bounds.Height, height));
        }
        return true;
    }

    public bool Connect(Room room)
    {
        if (Neighbors.Contains(room))
        {
            return true;
        }
        if (Neighbors.Count < Info.MaxConnections && room.Neighbors.Count < room.Info.MaxConnections)
        {
            var intersection = Bounds.Intersect(room.Bounds);
            if (intersection == null || (intersection.Width == 0 && intersection.Height >= 2) || (intersection.Height == 0 && intersection.Width >= 2))
            {
                var connectionPoint = GetConnectionPoint(room);
                if (connectionPoint == null)
                {
                    return false;
                }
                ConnectionPoints.Add(room, connectionPoint.Value);
                room.ConnectionPoints.Add(this, connectionPoint.Value);
                Neighbors.Add(room);
                room.Neighbors.Add(this);

                return true;
            }
        }
        return false;
    }

    private SerializableVector2Int? GetConnectionPoint(Room room)
    {
        var doorPoints = new List<SerializableVector2Int>();
        var intersection = Bounds.Intersect(room.Bounds);
        if (intersection == null)
        {
            return null;
        }
        foreach (var point in intersection.GetPoints())
        {
            if (CanConnect(point) && room.CanConnect(point))
            {
                doorPoints.Add(point);
            }
        }
        if (doorPoints.Count > 0)
        {
            return SeededRandom.PickRandom(doorPoints);
        }
        return null;

    }

    public virtual void Paint(Level level)
    {
        var painter = new RoomPainter(level, this);
        PaintOutsideWalls(level);
        painter.PaintFloorArea(Bounds.Left + 1, Bounds.Right - 2, Bounds.Bottom + 1, Bounds.Top - 2);
    }

    public virtual bool CanConnect(Vector2Int point)
    {
        if (point.x == Bounds.Left || point.x == Bounds.Right)
        {
            return point.y < Bounds.Top - 1 && point.y > Bounds.Bottom;
        }
        else if (point.y == Bounds.Top || point.y == Bounds.Bottom)
        {
            return point.x < Bounds.Right - 1 && point.x > Bounds.Left;
        }
        return false;
    }

    public virtual void PaintDoors(Level level)
    {
        var painter = new RoomPainter(level, this);
        foreach (var door in ConnectionPoints.Values)
        {
            if (door.x == Bounds.Right)
            {
                painter.PaintFloor(door.x - 1, door.y);
            }
            else if (door.y == Bounds.Top)
            {
                painter.PaintFloor(door.x, door.y - 1);
            }
            else
            {
                painter.PaintFloor(door.x, door.y);
            }
            if (door.x == Bounds.Right || door.x == Bounds.Left)
            {
                var xValue = door.x == Bounds.Right ? door.x - 1 : door.x;
                if (door.y <= Bounds.Top - 2)
                {
                    painter.PaintTopWall(xValue, xValue + 1, door.y + 1);
                }
                if (door.y > Bounds.Bottom + 1)
                {
                    if (door.x == Bounds.Right)
                    {
                        painter.PaintTerrainTile(xValue, door.y - 1, SeededRandom.PickRandom(Painter.TurnLeftTiles));
                    }
                    else
                    {
                        painter.PaintTerrainTile(xValue, door.y - 1, SeededRandom.PickRandom(Painter.TurnRightTiles));
                    }
                }
                else if (door.y == Bounds.Bottom + 1)
                {
                    painter.PaintBottomWall(xValue, xValue + 1, door.y - 1);
                }
            }
            else if (door.y == Bounds.Bottom)
            {
                if (door.x < Bounds.Right - 2)
                {
                    painter.PaintTerrainTile(door.x + 1, door.y, SeededRandom.PickRandom(Painter.TurnLeftTiles));
                }
                else if (door.x == Bounds.Right - 2)
                {
                    painter.PaintRightWall(door.y, door.y + 1, door.x + 1);
                }
                if (door.x > Bounds.Left + 1)
                {
                    painter.PaintTerrainTile(door.x - 1, door.y, SeededRandom.PickRandom(Painter.TurnRightTiles));
                }
                else if (door.x == Bounds.Left + 1)
                {
                    painter.PaintLeftWall(door.y, door.y + 1, door.x - 1);
                }
            }
        }
    }

    public virtual void Decorate(Level level)
    {
        var painter = new RoomPainter(level, this);
        painter.AutoDecorate();
    }

    public virtual List<Vector2Int> GetPossibleSpawnLocations(Level level)
    {
        var res = new List<Vector2Int>();
        var painter = new RoomPainter(level, this);
        for (int i = Bounds.Left + 1; i < Bounds.Right - 1; i++)
        {
            for (int j = Bounds.Bottom + 1; j < Bounds.Top - 1; j++)
            {
                if (painter.IsFloor(i, j))
                {
                    res.Add(new Vector2Int(i, j));
                }
            }
        }
        return res;
    }

    public virtual List<Trap> GetAnyRequiredTraps(Level level)
    {
        return new List<Trap>();
    }

    public virtual List<EnemyBody> GetAnyRequiredEnemies(Level level)
    {
        return new List<EnemyBody>();
    }

    public virtual List<TreasureChest> GetAnyRequiredTreasure(Level level)
    {
        return new List<TreasureChest>();
    }

    protected void PaintOutsideWalls(Level level)
    {
        var painter = new RoomPainter(level, this);
        painter.PaintBottomWall(Bounds.Left, Bounds.Right - 1, Bounds.Bottom);
        painter.PaintTopWall(Bounds.Left, Bounds.Right - 1, Bounds.Top - 1);
        painter.PaintLeftWall(Bounds.Bottom, Bounds.Top, Bounds.Left);
        painter.PaintRightWall(Bounds.Bottom, Bounds.Top, Bounds.Right - 1);
        painter.PaintBottomLeftCorner(Bounds.Left, Bounds.Bottom);
        painter.PaintBottomRightCorner(Bounds.Right - 1, Bounds.Bottom);
        Corners.Add(new Vector2Int(Bounds.Left, Bounds.Bottom));
        Corners.Add(new Vector2Int(Bounds.Right - 1, Bounds.Bottom));
        Corners.Add(new Vector2Int(Bounds.Left, Bounds.Top - 1));
        Corners.Add(new Vector2Int(Bounds.Right - 1, Bounds.Top - 1));
    }


}