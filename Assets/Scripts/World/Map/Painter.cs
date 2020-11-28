using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

class Painter
{
    private static System.Random random { get; set; } = new System.Random();
    public static List<Tile> LeftWallTiles { get; set; }
    public static List<Tile> TopWallTiles { get; set; }
    public static List<Tile> RightWallTiles { get; set; }
    public static List<Tile> BottomWallTiles { get; set; }
    public static List<Tile> FloorTiles { get; set; }
    public static List<Tile> TurnLeftTiles { get; set; }
    public static List<Tile> TurnRightTiles { get; set; }
    public static Tile BottomLeftCorner { get; set; }
    public static Tile BottomRightCorner { get; set; }
    public static Tile EmptySpace { get; set; }
    public static Tile LadderDown { get; set; }
    private static bool HaveTilesBeenLoaded { get; set; } = false;

    private static string Path = "Tiles/";
    private DecorativeTileMap decorations;

    public Painter(DecorativeTileMap decorativeTileMap)
    {
        LoadTiles();
        decorations = decorativeTileMap;
    }

    public static void LoadTiles()
    {
        if (HaveTilesBeenLoaded)
            return;

        TopWallTiles = LoadTilesWithName("TopWall", 4);
        LeftWallTiles = LoadTilesWithName("LeftWall", 4);
        RightWallTiles = LoadTilesWithName("RightWall", 4);
        BottomWallTiles = LoadTilesWithName("BottomWall", 4);
        FloorTiles = LoadTilesWithName("Floor", 4);
        TurnLeftTiles = LoadTilesWithName("TurnLeft", 2);
        TurnRightTiles = LoadTilesWithName("TurnRight", 2);
        BottomLeftCorner = LoadTile("BottomLeftCorner");
        BottomRightCorner = LoadTile("BottomRightCorner");
        EmptySpace = LoadTile("EmptySpace");
        LadderDown = LoadTile("LadderDown");

        HaveTilesBeenLoaded = true;
    }

    /// <summary>
    /// Paints the tileMap to match the roguelike map
    /// </summary>
    /// <param name="tileMap"></param>
    /// <param name="map"></param>
    public void Paint(Tilemap tileMap, Roguelike.Tile[,] map, int sizeX, int sizeZ)
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                Tile tile = null;
                switch (map[i, j].tileEntityType)
                {
                    //Enemies and the player can only spawn on floors
                    case Roguelike.Tile.TileEntityType.enemy:
                    case Roguelike.Tile.TileEntityType.player:
                    case Roguelike.Tile.TileEntityType.empty:
                        if (map[i, j].tileTerrainType == Roguelike.Tile.TileTerrainType.stairsDown)
                        {
                            decorations.PaintStairs(new Vector3Int(i, j, 0));
                        }
                        tile = FloorTiles.PickRandom();
                        break;
                    case Roguelike.Tile.TileEntityType.wall:
                        tile = GetWallTile(i, j, map);
                        break;
                }
                if (tile != null)
                {
                    tileMap.SetTile(new Vector3Int(i, j, 0), tile);
                    var getTile = tileMap.GetTile(new Vector3Int(i, j, 0));
                    Debug.Log(getTile.name);
                }
            }
        }
    }

    private Tile GetWallTile(int x, int y, Roguelike.Tile[,] map)
    {
        //This is terrible but no super easy way of doing it right now
        int maxX = map.GetLength(0) - 1;
        int maxY = map.GetLength(1) - 1;
        if (y != 0 && IsEmpty(map[x, y - 1]))
        {
            return TopWallTiles.PickRandom();
        }
        if (x != maxX && y != maxY && IsEmpty(map[x + 1, y]) && IsEmpty(map[x, y + 1]))
        {
            return TurnRightTiles.PickRandom();
        }
        if (x != 0 && y != maxY && IsEmpty(map[x - 1, y]) && IsEmpty(map[x, y + 1]))
        {
            return TurnLeftTiles.PickRandom();
        }
        if (x != 0 && IsEmpty(map[x - 1, y]))
        {
            return RightWallTiles.PickRandom();
        }
        if (x != maxX && IsEmpty(map[x + 1, y]))
        {
            return LeftWallTiles.PickRandom();
        }
        if (y != maxY && IsEmpty(map[x, y + 1]))
        {
            return BottomWallTiles.PickRandom();
        }
        if (x != maxX && y != 0 && IsEmpty(map[x + 1, y - 1]))
        {
            return LeftWallTiles.PickRandom();
        }
        if (x != 0 && y != 0 && IsEmpty(map[x - 1, y - 1]))
        {
            return RightWallTiles.PickRandom();
        }
        if (x != maxX && y != maxY && IsEmpty(map[x + 1, y + 1]))
        {
            return BottomLeftCorner;
        }
        if (x != 0 && y != maxY && IsEmpty(map[x - 1, y + 1]))
        {
            return BottomRightCorner;
        }

        return EmptySpace;


    }

    private static bool IsEmpty(Roguelike.Tile tile)
    {
        return tile.tileEntityType == Roguelike.Tile.TileEntityType.empty;
    }


    private static List<Tile> LoadTilesWithName(string baseNameOfTiles, int numOfTiles)
    {
        var res = new List<Tile>();
        for (int i = 0; i < numOfTiles; i++)
        {
            res.Add(LoadTile(baseNameOfTiles + i));
        }
        return res;
    }

    private static Tile LoadTile(string name)
    {
        try
        {
            return (Tile)Resources.Load($"{Path}{name}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Threw error: {e.GetType()} while trying to load tile: {name}");
            return null;
        }
    }
}

