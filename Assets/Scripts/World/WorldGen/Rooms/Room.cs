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
    SPECIAL
}

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

public abstract class Room
{
    public RogueRect Bounds { get; private set; }
    public RoomInfo Info { get; private set; }

    public List<Room> Neighbors { get; private set; }

    public Dictionary<Room, Vector2Int> ConnectionPoints { get; private set; }
    public List<Vector2Int> Corners { get; private set; }

    public Room(RoomInfo info)
    {
        Info = info;
        Bounds = new RogueRect();
        Neighbors = new List<Room>();
        ConnectionPoints = new Dictionary<Room, Vector2Int>();
        Corners = new List<Vector2Int>();
    }

    public void Reset()
    {
        Bounds = new RogueRect();
        ConnectionPoints = new Dictionary<Room, Vector2Int>();
        Corners = new List<Vector2Int>();
        foreach (var room in Neighbors)
        {
            room.Neighbors.Remove(this);
            room.ConnectionPoints.Remove(this);
        }
        Neighbors = new List<Room>();
    }

    public void SetSize()
    {
        var width = Random.Range(Info.MinWidth, Info.MaxWidth);
        var height = Random.Range(Info.MinHeight, Info.MaxHeight);
        Bounds.Resize(width, height);
    }

    public bool SetSizeWithLimit(int width, int height)
    {
        if (width <= Info.MinWidth || height <= Info.MinHeight)
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

    private Vector2Int? GetConnectionPoint(Room room)
    {
        var doorPoints = new List<Vector2Int>();
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
            return doorPoints.PickRandom();
        }
        return null;

    }

    public abstract void Paint(Level level);

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
        }
    }
}