using DungeonGenerator;
using DungeonGenerator.core;
using NesScripts.Controls.PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class FloorFactory
{
    //Generates a floor. Can be modified to see certain floors to have different types
    public static Floor GenerateFloor(int x, int y, Tilemap tileMap, int seed = 0)
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
        return new Floor(dungeonData, tileMap, seed);
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

    // Pathfinding
    private float[,] walkCostsMap = null; // This is the cost of that tile. 0 = impassable.
    public WalkableTilesGrid walkGrid = null;

    public Floor(Sirpgeon dungeonData, Tilemap tileMap, int seed)
    {
        this.dungeonData = dungeonData;
        this.tileMap = tileMap;
        this.seed = seed;
        BuildFloor();
    }

    //Builds the floor from the dungeon data
    private void BuildFloor()
    {
        DunNode[][] nodes = dungeonData.getNodes();

        sizeX = nodes.Length;
        sizeZ = nodes[0].Length;

        map = new Roguelike.Tile[sizeX, sizeZ];

        Tile topLeft = Resources.Load<Tile>("Tiles/GroundTopLeft");
        Tile topRight = Resources.Load<Tile>("Tiles/GroundTopRight");
        Tile bottomLeft = Resources.Load<Tile>("Tiles/GroundBottomLeft");
        Tile bottomRight = Resources.Load<Tile>("Tiles/GroundBottomRight");

        // Fill in the map
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                map[i, j] = new Roguelike.Tile();
                // For each element in nodes, translate it onto map.
                if (nodes[i][j].open())
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
                else
                {
                    SpawnWallAt(i, j);
                }
            }
        }

        // Spawn an enemy
        SpawnEnemy();
        SpawnEnemy();
        SpawnEnemy();
        SpawnEnemy();
        SpawnEnemy();
        SpawnEnemy();
        SpawnEnemy();
        SpawnEnemy();

        GenerateWalkableMap();

    }

    public void PlacePlayerInDungeon()
    {
        List<Room> rooms = dungeonData.getRooms();
        int x = rooms[0].bottomRight().x();
        int y = rooms[0].bottomRight().y();
        BattleManager.player.EstablishSelf(x, y);
        BattleManager.player.transform.position = new Vector3(x, 0.05f, y);
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
            Debug.LogError("BattleGrid--PickRandomEmptytile():: Uh... couldn't find a single empty tile. That seems unlikely.");
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

        Debug.LogError("BattleGrid--ForceFindEmptyTile():: And we couldn't even find one by looping through. What did you do to the game board!?!?");
        return Vector2Int.one;
    }

    public void SpawnEnemy()
    {
        Vector2Int spawnLoc = PickRandomEmptyTile();
        var newEnemy = BattleGrid.instance.SpawnEnemy(spawnLoc);
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
                walkGrid = new WalkableTilesGrid(walkCostsMap);
            }
        }
    }

    // Spawns a wall at this location for the corresponding Tile in the array.
    private void SpawnWallAt(int x, int z)
    {
        // Check to see if the tile we're trying to spawn on is empty
        if (map[x, z].tileEntityType != Roguelike.Tile.TileEntityType.empty)
        {
            Debug.Log("Battlegrid::SpawnWallAt(" + x + "," + z + ")--Tried to spawn a wall here, but that tile is not empty.");
            return;
        }

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
            Debug.LogError("Error, tried to move " + obj.name + ", but it isn't where it says it should be.");
            return;
        }

        // Remove it from its previous location.
        map[obj.xPos, obj.zPos].SetEntityOnTile(null);
        PlaceObjectOn(tar.x, tar.y, obj);
    }


}

