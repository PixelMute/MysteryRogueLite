﻿using System.Collections.Generic;
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
        var entrance = new Entrance();
        var exit = new Exit();
        rooms.Add(entrance);
        rooms.Add(exit);

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
            for (int i = 0; i < room.ConnectionLocations.Count; i++)
            {
                room.ConnectionLocations[i] += new Vector2Int(-xOffset, -yOffset);
            }
        }
    }
}

