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
        for (int i = 0; i < bounds.Width; i++)
        {
            for (int j = 0; j < bounds.Height; j++)
            {
                var tile = tileMap.GetTile(new UnityEngine.Vector3Int(i, j, 0));
                data.TileNames[j, i] = tile == null ? "" : tile.name;
            }
        }
        return data;
    }

    public static void LoadTileMap(TileMapData data, Tilemap tileMap)
    {
        for (int i = 0; i < data.Bounds.Width; i++)
        {
            for (int j = 0; j < data.Bounds.Height; j++)
            {
                tileMap.SetTile(new Vector3Int(i, j, 0), LoadTile(data.TileNames[j, i]));
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

