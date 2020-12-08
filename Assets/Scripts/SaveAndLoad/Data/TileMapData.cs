using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
class TileMapData
{
    private static Dictionary<string, Tile> tiles = new Dictionary<string, Tile>();
    public static string Path = "Tiles/";
    string[,] TileNames;
    int Left;
    int Bottom;
    int Width;
    int Height;

    public static TileMapData SaveTileMap(Tilemap tileMap, int left, int bottom, int width, int height)
    {
        var data = new TileMapData()
        {
            TileNames = new string[height, width],
            Left = left,
            Bottom = bottom,
            Width = width,
            Height = height
        };
        for (int i = left; i < left + width; i++)
        {
            for (int j = bottom; j < bottom + height; j++)
            {
                data.TileNames[i, j] = tileMap.GetTile(new UnityEngine.Vector3Int(i, j, 0)).name;
            }
        }
        return data;
    }

    public static void LoadTileMap(TileMapData data, Tilemap tileMap)
    {
        for (int i = data.Left; i < data.Left + data.Width; i++)
        {
            for (int j = data.Bottom; j < data.Bottom + data.Height; j++)
            {
                tileMap.SetTile(new UnityEngine.Vector3Int(i, j, 0), LoadTile(data.TileNames[i, j]));
            }
        }
    }

    private static Tile LoadTile(string name)
    {
        if (tiles.ContainsKey(name))
        {
            return tiles[name];
        }
        var tile = (Tile)Resources.Load($"{Path}{name}");
        tiles.Add(name, tile);
        return tile;
    }
}

