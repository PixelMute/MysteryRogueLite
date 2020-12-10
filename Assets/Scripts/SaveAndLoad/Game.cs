using System;

[Serializable]
public class Game
{
    PlayerData Player;
    TileMapData Terrain;
    TileMapData Decorations;
    FloorManager FloorManager;

    public static Game SaveGame()
    {
        var game = new Game()
        {
            Player = PlayerData.SavePlayer(BattleManager.player),
            Terrain = TileMapData.SaveTileMap(BattleGrid.instance.CurrentFloor.Level.Terrain, BattleGrid.instance.CurrentFloor.Level.Bounds),
            Decorations = TileMapData.SaveTileMap(DecorativeTileMap.instance.TileMap, BattleGrid.instance.CurrentFloor.Level.Bounds),
            FloorManager = BattleGrid.instance.floorManager,
        };
        return game;

    }
}

public class GameMetaData
{
    public int GameSlot;
    public int FloorNumber;
    public DateTime LastPlayed;
}

