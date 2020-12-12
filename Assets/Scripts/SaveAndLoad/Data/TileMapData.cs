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
    RogueRect Bounds;

    public static TileMapData SaveTileMap(Tilemap tileMap, RogueRect bounds)
    {
        var data = new TileMapData()
        {
            TileNames = new string[bounds.Height, bounds.Width],
            Bounds = bounds,
        };
        for (int i = bounds.Left; i < bounds.Right; i++)
        {
            for (int j = bounds.Bottom; j < bounds.Top; j++)
            {
                var tile = tileMap.GetTile(new UnityEngine.Vector3Int(i, j, 0));
                data.TileNames[j - bounds.Bottom, i - bounds.Left] = tile == null ? "" : tile.name;
            }
        }
        return data;
    }

    public static void LoadTileMap(TileMapData data, Tilemap tileMap)
    {
        for (int i = data.Bounds.Left; i < data.Bounds.Right; i++)
        {
            for (int j = data.Bounds.Bottom; j < data.Bounds.Top; j++)
            {
                tileMap.SetTile(new Vector3Int(i, j, 0), LoadTile(data.TileNames[j - data.Bounds.Bottom, i - data.Bounds.Left]));
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

