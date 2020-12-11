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
        };
        return game;
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

    public void LoadGame()
    {
        TileMapData.LoadTileMap(Terrain, BattleGrid.instance.tileMap);
        var fogOfWar = UnityEngine.Object.FindObjectOfType<FogOfWar>();
        TileMapData.LoadTileMap(FogOfWar, fogOfWar.TileMap);
        TileMapData.Path = "Tiles/Decorations/";
        TileMapData.LoadTileMap(Decorations, BattleGrid.instance.DecorativeTileMap.TileMap);
        RestoreTorches();
        SetStairs();
        BattleGrid.instance.floorManager = FloorManager;
        InstantiateFloor(BattleGrid.instance.floorManager.CurrentFloor);
        BattleManager.player.puim.ClearCards();
        PlayerData.LoadPlayer(Player, BattleManager.player);
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
        floor.enemies = new List<EnemyBody>();
        RestoreTraps(floor);
        floor.PlacePlayerInDungeon();
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
}

public class GameMetaData
{
    public int GameSlot;
    public int FloorNumber;
    public DateTime LastPlayed;
}

