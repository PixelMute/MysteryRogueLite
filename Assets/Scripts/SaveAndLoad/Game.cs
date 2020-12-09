using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

[Serializable]
public class Game
{
    PlayerData Player;
    TileMapData Terrain;
    TileMapData Decorations;
    FloorManager FloorManager;

    public static void SaveGame()
    {
        var game = new Game()
        {
            Player = PlayerData.SavePlayer(BattleManager.player),
            Terrain = TileMapData.SaveTileMap(BattleGrid.instance.CurrentFloor.Level.Terrain, BattleGrid.instance.CurrentFloor.Level.Bounds),
            Decorations = TileMapData.SaveTileMap(DecorativeTileMap.instance.TileMap, BattleGrid.instance.CurrentFloor.Level.Bounds),
            FloorManager = BattleGrid.instance.floorManager,
        };
        Debug.Log(GetSavePath("0"));
        using (var fileStream = new FileStream(GetSavePath("0"), FileMode.Create))
        {
            var binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, game);
        }
    }

    private static string GetSavePath(string index)
    {
        return Path.Combine(Application.persistentDataPath, $"{index}.save");

    }
}

