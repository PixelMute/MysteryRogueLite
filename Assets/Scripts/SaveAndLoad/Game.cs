using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Game
{
    PlayerData Player;
    TileMapData Terrain;
    TileMapData Decorations;
    TileMapData FogOfWar;
    FloorManager FloorManager;
    List<(SerializableVector2Int, bool)> Traps;
    List<(SerializableVector2Int, DecorativeTileMap.TorchDirection)> Torches;
    SerializableVector2Int StairsLocation;
    List<EnemyData> Enemies;
    List<(SerializableVector2Int, TreasureChest.TreasureChestTypeEnum)> Treasure;
    List<(SerializableVector2Int, int)> Coins;

    public static Game SaveGame()
    {
        var fogOfWar = UnityEngine.Object.FindObjectOfType<FogOfWar>();
        var game = new Game()
        {
            Player = PlayerData.SavePlayer(BattleManager.player),
            Terrain = TileMapData.SaveTileMap(BattleGrid.instance.CurrentFloor.Level.Terrain, BattleGrid.instance.CurrentFloor.Level.Bounds),
            Decorations = TileMapData.SaveTileMap(DecorativeTileMap.instance.TileMap, BattleGrid.instance.CurrentFloor.Level.Bounds),
            FogOfWar = TileMapData.SaveTileMap(fogOfWar.TileMap, fogOfWar.Bounds),
            FloorManager = BattleGrid.instance.floorManager,
            Traps = SaveTraps(),
            Torches = SaveTorches(),
            StairsLocation = BattleGrid.instance.CurrentFloor.Level.Exit.StairsLocation.Value,
            Enemies = SaveEnemies(),
        };
        game.SaveItems(BattleGrid.instance.CurrentFloor);
        return game;
    }

    private static List<EnemyData> SaveEnemies()
    {
        var res = new List<EnemyData>();
        foreach (var enemy in BattleGrid.instance.CurrentFloor.enemies)
        {
            res.Add(EnemyData.SaveEnemy(enemy));
        }
        return res;
    }

    private List<EnemyBody> LoadEnemies()
    {
        var res = new List<EnemyBody>();
        foreach (var enemy in Enemies)
        {
            res.Add(EnemyData.LoadEnemy(enemy));
        }
        return res;
    }

    private static List<(SerializableVector2Int, bool)> SaveTraps()
    {
        var traps = BattleGrid.instance.CurrentFloor.traps;
        var res = new List<(SerializableVector2Int, bool)>();
        foreach (var trap in traps)
        {
            res.Add((new SerializableVector2Int(trap.xPos, trap.zPos), trap.Invisible));
        }
        return res;
    }

    private static List<(SerializableVector2Int, DecorativeTileMap.TorchDirection)> SaveTorches()
    {
        var torches = BattleGrid.instance.DecorativeTileMap.torches;
        var res = new List<(SerializableVector2Int, DecorativeTileMap.TorchDirection)>();
        foreach (var (torch, direction) in torches)
        {
            res.Add((BattleManager.ConvertVector(torch.transform.position), direction));
        }
        return res;
    }

    private void SaveItems(Floor floor)
    {
        Treasure = new List<(SerializableVector2Int, TreasureChest.TreasureChestTypeEnum)>();
        Coins = new List<(SerializableVector2Int, int)>();
        for (int i = 0; i < floor.sizeX; i++)
        {
            for (int j = 0; j < floor.sizeZ; j++)
            {
                if (floor.map[i, j].tileItemType == Roguelike.Tile.TileItemType.money)
                {
                    var money = floor.map[i, j].GetItemOnTile() as DroppedMoney;
                    Coins.Add((new SerializableVector2Int(i, j), money.Value));
                }
                else if (floor.map[i, j].tileItemType == Roguelike.Tile.TileItemType.smallChest)
                {
                    var chest = floor.map[i, j].GetItemOnTile() as TreasureChest;
                    Treasure.Add((new SerializableVector2Int(i, j), chest.ChestType));
                }
            }
        }
    }

    public void LoadGame()
    {
        TileMapData.LoadTileMap(Terrain, BattleGrid.instance.tileMap);
        RestoreTorches();
        SetStairs();
        BattleGrid.instance.floorManager = FloorManager;
        InstantiateFloor(BattleGrid.instance.floorManager.CurrentFloor);
        BattleManager.player.puim.ClearCards();
        PlayerData.LoadPlayer(Player, BattleManager.player);
        BattleGrid.instance.floorManager.CurrentFloor.PlacePlayerInDungeon();
        var fogOfWar = UnityEngine.Object.FindObjectOfType<FogOfWar>();
        TileMapData.LoadTileMap(FogOfWar, fogOfWar.TileMap);
        TileMapData.Path = "Tiles/Decorations/";
        TileMapData.LoadTileMap(Decorations, BattleGrid.instance.DecorativeTileMap.TileMap);
        fogOfWar.FindElementsToHide();
        LoadItems();
        Painter.LoadTiles();
    }

    private void SetStairs()
    {
        var location = new Vector3Int(StairsLocation.x, StairsLocation.y, 0);
        DecorativeTileMap.instance.PaintStairs(location);
    }

    private void InstantiateFloor(Floor floor)
    {
        floor.SpawnWalls();
        floor.GenerateWalkableMap();
        floor.enemies = LoadEnemies();
        RestoreTraps(floor);
        floor.Level.Terrain = BattleGrid.instance.tileMap;
        floor.Level.Decorations = BattleGrid.instance.DecorativeTileMap.TileMap;
    }

    private void RestoreTraps(Floor floor)
    {
        floor.traps = new List<Trap>();
        foreach (var (location, invisble) in Traps)
        {
            var spikes = EnemySpawner.SpawnSpikes(location).GetComponent<Spikes>();
            floor.traps.Add(spikes);
            floor.PlaceTerrainOn(location.x, location.y, spikes);
            if (invisble)
            {
                spikes.MakeInvisible();
            }
        }
    }

    private void RestoreTorches()
    {
        foreach (var (torch, direction) in Torches)
        {
            switch (direction)
            {
                case DecorativeTileMap.TorchDirection.regular:
                    DecorativeTileMap.SpawnTorch(torch.x, torch.y);
                    break;
                case DecorativeTileMap.TorchDirection.left:
                    DecorativeTileMap.SpawnSideTorch(torch.x, torch.y, false);
                    break;
                default:
                    DecorativeTileMap.SpawnSideTorch(torch.x, torch.y, true);
                    break;
            }
        }
    }

    private void LoadItems()
    {
        foreach (var (location, value) in Coins)
        {
            BattleGrid.instance.ForceSpawnMoney(location, value);
        }
        foreach (var (location, type) in Treasure)
        {
            BattleGrid.instance.CurrentFloor.SpawnChestAt(location.x, location.y, type);
        }
    }
}

public class GameMetaData
{
    public int GameSlot;
    public int FloorNumber;
    public DateTime LastPlayed;
}

