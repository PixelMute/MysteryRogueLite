using DungeonGenerator;
using DungeonGenerator.core;
using NesScripts.Controls.PathFind;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorManager
{
    public int CurrentFloorNumber { get; private set; } = 0;
    public List<Floor> AllFloors { get; private set; } = new List<Floor>();
    public Floor CurrentFloor { get; private set; }

    public void GenerateNewFloor()
    {
        CurrentFloor = GenerateRandomFloor(CurrentFloorNumber);
        AllFloors.Append(CurrentFloor);
        CurrentFloor.InstantiateFloor();
    }

    public void GoDownFloor()
    {
        CurrentFloor.DespawnFloor();
        CurrentFloorNumber++;
        if (CurrentFloorNumber < AllFloors.Count)
        {
            CurrentFloor = AllFloors[CurrentFloorNumber];
            CurrentFloor.InstantiateFloor();
        }
        else
        {
            GenerateNewFloor();
        }
    }

    //Generates a floor. Can be modified to see certain floors to have different types
    public Floor GenerateRandomFloor(int floorNumber, int x = 60, int y = 60, int seed = 0)
    {
        //Allows us to create the exact same floor if we know the seed
        if (seed == 0)
        {
            seed = UnityEngine.Random.Range(-10000, 100000);
        }
        var dungeonData = new Sirpgeon(x, y, seed, DungeonGenerator.gen.hints.GenBorder.ODD, DungeonGenerator.gen.hints.GenDensity.DENSE,
            DungeonGenerator.gen.hints.GenRoomShape.SPLATTER, DungeonGenerator.gen.hints.GenRoomSize.MEDIUM,
            DungeonGenerator.gen.hints.GenMazeType.DEPTH_FIRST, DungeonGenerator.gen.hints.GenPathing.TRUNCATE,
            DungeonGenerator.gen.hints.GenEgress.RANDOM_PLACEMENT, 3, 1);
        return new Floor(dungeonData, floorNumber, seed);
    }
}


public class Floor
{
    private Tilemap tileMap;
    private Sirpgeon dungeonData;
    public Roguelike.Tile[,] map { get; private set; }
    public int sizeX { get; private set; }
    public int sizeZ { get; private set; }
    public int seed { get; private set; }
    public int FloorNumber { get; private set; }
    public List<GenericEnemy> enemies { get; private set; } = new List<GenericEnemy>();

    private Vector2Int stairsUpLocation;
    private Vector2Int stairsDownLocation;

    // Pathfinding
    private float[,] walkCostsMap = null; // This is the cost of that tile. 0 = impassable.
    public WalkableTilesGrid walkGrid = null;

    public Floor(Sirpgeon dungeonData, int floorNumber, int seed)
    {
        this.dungeonData = dungeonData;
        tileMap = BattleGrid.instance.tileMap;
        this.seed = seed;
        FloorNumber = floorNumber;
        BuildFloor();
    }

    //Builds the floor from the dungeon data
    public void BuildFloor()
    {
        DunNode[][] nodes = dungeonData.getNodes();

        sizeX = nodes.Length;
        sizeZ = nodes[0].Length;

        map = new Roguelike.Tile[sizeX, sizeZ];

        // Fill in the map
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                map[i, j] = new Roguelike.Tile();
                // If it isn't an empty space, it should be a wall
                if (!nodes[i][j].open())
                {
                    map[i, j].tileEntityType = Roguelike.Tile.TileEntityType.wall;
                }
            }
        }

        // Set the enemies locations
        SetEnemyLocation();
        SetEnemyLocation();
        SetEnemyLocation();
        SetEnemyLocation();
        SetEnemyLocation();
        SetEnemyLocation();
        SetEnemyLocation();
        SetEnemyLocation();

        SetStairsLocation();
    }

    public void InstantiateFloor()
    {
        Tile topLeft = Resources.Load<Tile>("Tiles/GroundTopLeft");
        Tile topRight = Resources.Load<Tile>("Tiles/GroundTopRight");
        Tile bottomLeft = Resources.Load<Tile>("Tiles/GroundBottomLeft");
        Tile bottomRight = Resources.Load<Tile>("Tiles/GroundBottomRight");

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                switch (map[i, j].tileEntityType)
                {
                    case Roguelike.Tile.TileEntityType.wall:
                        SpawnWallAt(i, j);
                        break;
                    case Roguelike.Tile.TileEntityType.enemy:
                        SpawnEnemyAt(i, j);
                        break;
                    //case Roguelike.Tile.TileEntityType.stairsUp:
                    //    var spawnLoc = new Vector2Int(i, j);
                    //    var stairsUp = BattleGrid.instance.SpawnStairsUp(spawnLoc);
                    //    PlaceObjectOn(spawnLoc.x, spawnLoc.y, stairsUp);
                    //    break;
                    case Roguelike.Tile.TileEntityType.stairsDown:
                        var spawnLoc = new Vector2Int(i, j);
                        var stairsDown = BattleGrid.instance.SpawnStairsDown(spawnLoc);
                        PlaceObjectOn(spawnLoc.x, spawnLoc.y, stairsDown);
                        break;
                }
                if (map[i, j].tileEntityType != Roguelike.Tile.TileEntityType.wall)
                {
                    Tile tile;
                    if (i % 2 == 0)
                    {
                        tile = j % 2 == 0 ? topLeft : topRight;
                    }
                    else
                    {
                        tile = j % 2 == 0 ? bottomLeft : bottomRight;
                    }
                    tileMap.SetTile(new Vector3Int(i, j, 0), tile);
                }
            }
        }

        PlacePlayerInDungeon();

        GenerateWalkableMap();
    }

    public void DespawnFloor()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                if (map[i, j].tileEntityType != Roguelike.Tile.TileEntityType.player)
                {
                    BattleGrid.instance.DestroyGameObject(map[i, j].GetEntityOnTile()?.gameObject);
                }
            }
        }
        tileMap.ClearAllTiles();
    }

    private void SetEnemyLocation()
    {
        Vector2Int spawnLoc = PickRandomEmptyTile();
        map[spawnLoc.x, spawnLoc.y].tileEntityType = Roguelike.Tile.TileEntityType.enemy;
    }

    private void SetStairsLocation()
    {
        Vector2Int spawnLoc = PickRandomEmptyTile();
        map[spawnLoc.x, spawnLoc.y].tileEntityType = Roguelike.Tile.TileEntityType.stairsDown;
        stairsDownLocation = spawnLoc;
        //if (FloorNumber != 0)
        //{
        //    spawnLoc = PickRandomEmptyTile();
        //    map[spawnLoc.x, spawnLoc.y].tileEntityType = Roguelike.Tile.TileEntityType.stairsUp;
        //    stairsUpLocation = spawnLoc;
        //}
    }

    public void PlacePlayerInDungeon()
    {
        var spawnLocation = PickRandomEmptyTile();
        BattleManager.player.EstablishSelf(spawnLocation.x, spawnLocation.y);
        BattleManager.player.transform.position = new Vector3(spawnLocation.x, 0.05f, spawnLocation.y);
        BattleManager.player.moveTarget = new Vector3(spawnLocation.x, BattleManager.player.transform.position.y, spawnLocation.y);
        BattleManager.player.isMoving = false;
    }

    private void SpawnStairs()
    {
        //List<Room> rooms = dungeonData.getRooms();
        //var spawnLoc = new Vector2Int(rooms.Last().bottomRight().x(), rooms.Last().bottomRight().y());
        var spawnLoc = PickRandomEmptyTile();
        var stairsDown = BattleGrid.instance.SpawnStairsDown(spawnLoc);
        PlaceObjectOn(spawnLoc.x, spawnLoc.y, stairsDown);
        stairsDownLocation = spawnLoc;
        if (FloorNumber != 0)
        {
            //spawnLoc = new Vector2Int(rooms[0].bottomRight().x(), rooms[0].bottomRight().y());
            spawnLoc = PickRandomEmptyTile();
            var stairsUp = BattleGrid.instance.SpawnStairsUp(spawnLoc);
            PlaceObjectOn(spawnLoc.x, spawnLoc.y, stairsUp);
            stairsUpLocation = spawnLoc;
        }
    }

    // Picks a random empty tile out of the map.
    public Vector2Int PickRandomEmptyTile()
    {
        int tarX;
        int tarZ;

        bool found = false;
        int tryNum = 0;
        do
        {
            tarX = UnityEngine.Random.Range(1, sizeX - 1);
            tarZ = UnityEngine.Random.Range(1, sizeZ - 1);

            if (map[tarX, tarZ].tileEntityType == Roguelike.Tile.TileEntityType.empty)
                found = true;
            tryNum += 1;
        } while (!found && tryNum < 100);

        if (tryNum >= 100)
        {
            UnityEngine.Debug.LogError("BattleGrid--PickRandomEmptytile():: Uh... couldn't find a single empty tile. That seems unlikely.");
            return ForceFindEmptyTile();
        }
        return new Vector2Int(tarX, tarZ);
    }

    // If the random method fails, loop through and find one by force
    private Vector2Int ForceFindEmptyTile()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                if (map[i, j].tileEntityType == Roguelike.Tile.TileEntityType.empty)
                {
                    return new Vector2Int(i, j);
                }
            }
        }

        UnityEngine.Debug.LogError("BattleGrid--ForceFindEmptyTile():: And we couldn't even find one by looping through. What did you do to the game board!?!?");
        return Vector2Int.one;
    }

    public void SpawnEnemy()
    {
        Vector2Int spawnLoc = PickRandomEmptyTile();
        SpawnEnemyAt(spawnLoc.x, spawnLoc.y);
    }

    private void SpawnEnemyAt(int x, int z)
    {
        var spawnLoc = new Vector2Int(x, z);
        var newEnemy = BattleGrid.instance.SpawnEnemy(spawnLoc);
        enemies.Add(newEnemy);
        newEnemy.name = "EnemyID: " + enemies.Count;
        PlaceObjectOn(spawnLoc.x, spawnLoc.y, newEnemy);
    }

    // Recalculates what is walkable and what is not.
    private void GenerateWalkableMap()
    {
        if (walkCostsMap == null)
        {
            // need to assign it.
            walkCostsMap = new float[sizeX, sizeZ];
        }

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                walkCostsMap[i, j] = map[i, j].GetPathfindingCost();
            }
        }
        walkGrid = new WalkableTilesGrid(walkCostsMap);
    }

    // Spawns a wall at this location for the corresponding Tile in the array.
    private void SpawnWallAt(int x, int z)
    {
        var entityTile = BattleGrid.instance.SpawnWall(x, z);
        PlaceObjectOn(x, z, entityTile);
    }

    // Sets obj's internal tracking of position, as well as updates the battlegrid.
    public void PlaceObjectOn(int x, int z, TileEntity obj)
    {

        float oldCost;
        if (walkCostsMap != null) // Might have to recalculate now that something has moved.
        {
            if (map[x, z].tileEntityType == Roguelike.Tile.TileEntityType.empty)
                oldCost = 0f;
            else
                oldCost = map[x, z].GetEntityOnTile().GetPathfindingCost();
            if (oldCost != obj.GetPathfindingCost()) // Need to update
            {
                walkCostsMap[x, z] = obj.GetPathfindingCost();
                walkGrid.UpdateGrid(walkCostsMap);
            }
        }

        obj.xPos = x;
        obj.zPos = z;
        map[x, z].SetEntityOnTile(obj);

    }

    // Moves the given object from wherever it is to the given location on the battlegrid.
    public void MoveObjectTo(Vector2Int tar, TileEntity obj)
    {
        if (obj != map[obj.xPos, obj.zPos].GetEntityOnTile())
        {
            UnityEngine.Debug.LogError("Error, tried to move " + obj.name + ", but it isn't where it says it should be.");
            return;
        }

        // Remove it from its previous location.
        map[obj.xPos, obj.zPos].SetEntityOnTile(null);
        PlaceObjectOn(tar.x, tar.y, obj);
    }


}

