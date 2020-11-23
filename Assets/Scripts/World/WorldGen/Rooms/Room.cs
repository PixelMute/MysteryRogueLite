using System;
using System.Collections.Generic;

public enum Direction
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

public class Room
{
    public Rect Bounds { get; private set; }
    public RoomInfo Info { get; private set; }

    public List<Room> Neighbors { get; private set; }

    public Room(RoomInfo info)
    {
        Info = info;
        Bounds = new Rect();
        Neighbors = new List<Room>();
    }

    public void Reset()
    {
        Bounds = new Rect();
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
        var intersection = Bounds.Intersect(room.Bounds);
        if ((intersection.Width == 0 && intersection.Height >= 2) || (intersection.Height == 0 && intersection.Width >= 2))
        {
            Neighbors.Add(room);
            room.Neighbors.Add(this);
            return true;
        }
        return false;
    }
}