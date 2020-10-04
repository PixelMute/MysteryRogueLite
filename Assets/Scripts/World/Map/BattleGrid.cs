using NesScripts.Controls.PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using Roguelike;
using TMPro;
using UnityEditor;
using UnityEngine;
using DungeonGenerator;
using DungeonGenerator.core;
using FoW;

// The battlegrid holds data relevant to one floor of a dungeon
public class BattleGrid : MonoBehaviour
{
    [HideInInspector] public static BattleGrid instance;
    [HideInInspector] public Tile[,] map;
    [HideInInspector] private DungeonGenerator.Sirpgeon dungeonData;
    [HideInInspector] public int sizeX;
    [HideInInspector] public int sizeZ;

    public GameObject wallPrefab;
    private GameObject terrainHolder;

    // Enemy stuff
    [HideInInspector] public List<GenericEnemy> enemies;
    public GameObject enemyPrefab;

    // Pathfinding
    [HideInInspector] private float[,] walkCostsMap = null; // This is the cost of that tile. 0 = impassable.
    [HideInInspector] public WalkableTilesGrid walkGrid = null;

    // Line of Sight
    public LayerMask sightBlockingLayer; // Things that block LOS are on this layer

    // Fog of War
    FogOfWarTeam fogOfWar;
    FogOfWarUnit playerReveal;

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        PlacePlayerInDungeon();
    }

    public void GenerateFloor(int x, int y)
    {
        int seed = UnityEngine.Random.Range(-10000, 100000);
        dungeonData = new Sirpgeon(x, y, seed, DungeonGenerator.gen.hints.GenBorder.ODD, DungeonGenerator.gen.hints.GenDensity.DENSE,
            DungeonGenerator.gen.hints.GenRoomShape.SPLATTER, DungeonGenerator.gen.hints.GenRoomSize.MEDIUM,
            DungeonGenerator.gen.hints.GenMazeType.DEPTH_FIRST,DungeonGenerator.gen.hints.GenPathing.TRUNCATE,
            DungeonGenerator.gen.hints.GenEgress.RANDOM_PLACEMENT, 3, 1);

        terrainHolder = GameObject.Find("TerrainHolder");

        DunNode[][] nodes = dungeonData.getNodes();

        sizeX = nodes.Length;
        sizeZ = nodes[0].Length;

        map = new Tile[sizeX, sizeZ];

        

        // Fill in the map
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                map[i, j] = new Tile();
                // For each element in nodes, translate it onto map.
                if (nodes[i][j].open())
                {
                    // This is an open space
                }
                else
                {
                    SpawnWallAt(i, j);
                }
            }
        }

        fogOfWar = GameObject.Find("FogOfWarHolder").GetComponent<FogOfWarTeam>();
        playerReveal = BattleManager.player.gameObject.GetComponentInChildren<FogOfWarUnit>();
        playerReveal.transform.SetParent(null);
        playerReveal.transform.position = BattleManager.player.transform.position;
        SetFogOfWarBounds();


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

    // Sets up the boundary for fog of war based on the map size.
    private void SetFogOfWarBounds()
    {
        fogOfWar.mapResolution = new Vector2Int(sizeX, sizeZ);
        fogOfWar.mapSize = Mathf.Max(sizeX, sizeZ);
        fogOfWar.mapOffset = new Vector2(sizeX / 2, sizeZ / 2);
        fogOfWar.Reinitialize();
    }

    private void PlacePlayerInDungeon()
    {
        List<Room> rooms = dungeonData.getRooms();
        int x = rooms[0].bottomRight().x();
        int y = rooms[0].bottomRight().y();
        BattleManager.player.EstablishSelf(x, y);
        BattleManager.player.transform.position = new Vector3(x, 0.05f, y);
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

    internal void ProcessEnemyTurn()
    {
        List<GenericEnemy> enemiesLeftToAct = new List<GenericEnemy>();
        // Add all enemies to this list.
        enemiesLeftToAct.AddRange(enemies);

        while (enemiesLeftToAct.Count > 0)
        {
            enemiesLeftToAct[0].ProcessTurn();
            enemiesLeftToAct.RemoveAt(0);
        }
    }

    // Spawns an enemy in a random location
    public void SpawnEnemy()
    {
        Vector2Int spawnLoc = PickRandomEmptyTile();
        // NOTE: used to have offset of 0.5 here
        GameObject enemyObj = Instantiate(enemyPrefab, BattleManager.ConvertVector(spawnLoc, transform.position.y + 0.05f), Quaternion.identity, transform);
        GenericEnemy newEnemy = enemyObj.GetComponent<GenericEnemy>();
        enemies.Add(newEnemy);
        newEnemy.name = "EnemyID: " + enemies.Count;
        PlaceObjectOn(spawnLoc.x, spawnLoc.y, newEnemy);
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

    // Sets obj's internal tracking of position, as well as updates the battlegrid.
    public void PlaceObjectOn(int x, int z, TileEntity obj)
    {
        float oldCost;
        if (walkCostsMap != null) // Might have to recalculate now that something has moved.
        {
            if (map[x, z].tileEntityType == Tile.TileEntityType.empty)
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

    /*public void MoveObjectTo(Vector3 target, TileEntity obj)
    {
        MoveObjectTo((int)target.x, (int)target.z, obj);
    }*/

    // Removes everything on given tile
    public void ClearTile(Vector2Int target)
    {
        if (map[target.x, target.y].tileEntityType == Tile.TileEntityType.enemy)
        {
            // Remove this from the list of enemies.
            enemies.Remove((GenericEnemy)map[target.x, target.y].GetEntityOnTile());
        }
        map[target.x, target.y].SetEntityOnTile(null);
    }

    // Spawns a wall at this location for the corresponding Tile in the array.
    private void SpawnWallAt(int x, int z)
    {
        // Check to see if the tile we're trying to spawn on is empty
        if (map[x,z].tileEntityType != Tile.TileEntityType.empty)
        {
            Debug.Log("Battlegrid::SpawnWallAt(" + x + "," + z + ")--Tried to spawn a wall here, but that tile is not empty.");
            return;
        }

        Vector3 spawnLocation = new Vector3(x, transform.position.y + 0.5f, z);
        GameObject go = Instantiate(wallPrefab, spawnLocation, Quaternion.identity, terrainHolder.transform);
        Wall entityTile = go.AddComponent<Wall>();
        PlaceObjectOn(x, z, entityTile);

        // spawn it graphically
        // NOTE: Used to have offset of 0.5f here
       
    }

    // You have LoS on a tile if you can draw a line from your center to one of the corners of the tile.
    public bool CheckLoS(Vector3 pos1, Vector3 pos2)
    {
        if (!Physics.Linecast(pos1, new Vector3(pos2.x + 0.5f, pos2.y, pos2.z), sightBlockingLayer) || !Physics.Linecast(pos1, new Vector3(pos2.x - 0.5f, pos2.y, pos2.z), sightBlockingLayer) ||
                !Physics.Linecast(pos1, new Vector3(pos2.x, pos2.y, pos2.z + 0.5f), sightBlockingLayer) || !Physics.Linecast(pos1, new Vector3(pos2.x, pos2.y, pos2.z - 0.5f), sightBlockingLayer))
            return true;
        else
            return false;
    }

    // This only checks the center to center LoS. Used for non-corner cutting attacks.
    public bool CheckSimpleLoS(Vector3 pos1, Vector3 pos2)
    {
        if (!Physics.Linecast(pos1, pos2, sightBlockingLayer))
            return true;
        else
            return false;
    }

    // Recalculates the LOS grid for looking in [size] squares in all directions.
    public bool[,] RecalculateLOS(int size, out bool[,] simpleLoSGrid)
    {
        bool[,] LoSGrid = GenerateLOSGrid(size, out simpleLoSGrid);
        Debug.Log("Updating fog of war");
        playerReveal.transform.position = BattleManager.player.moveTarget;
        fogOfWar.ManualUpdate(1f);
        return LoSGrid;
    }


    public enum AcceptableTileTargetFailReasons {success, mustTargetCreature };
    // Returns true if this tile is an acceptable target.
    // Only checks conditions POST-HIGHLIGHTING check
    public AcceptableTileTargetFailReasons AcceptableTileTarget(Vector2Int tileTarget, Card cardData)
    {
        bool needsToTargetCreature = false;
        foreach (Card.PlayCondition x in cardData.conditions)
        {
            if (x == Card.PlayCondition.mustHitCreature)
                needsToTargetCreature = true;
        }

        Tile targetedTile = map[tileTarget.x, tileTarget.y];

        if (needsToTargetCreature)
        {
            // Check if this tile has an entity on it.
            if (!targetedTile.IsCreatureOnTile())
                return AcceptableTileTargetFailReasons.mustTargetCreature;
        }

        // If we've gotten down here, we're successful
        return AcceptableTileTargetFailReasons.success;
    }

    // Damages whatever is on the given tile. If it's got HP, we'll smack it.
    /*public void StrikeTile(Vector3 target, float damage)
    {
        StrikeTile(new Vector2Int((int)target.x, (int)target.z), damage);
    }*/

    public void StrikeTile(Vector2Int target, int damage)
    {
        Debug.Log("Attacking tile " + target + " for " + damage + " damage.");
        Tile targetTile = map[target.x, target.y];
        if (targetTile.tileEntityType == Tile.TileEntityType.empty)
        {
            return;
        }

        targetTile.GetEntityOnTile().TakeDamage(damage);
    }

    public AcceptableTileTargetFailReasons AcceptableTileTarget(Vector3 tileTarget, Card cardData)
    {
        return AcceptableTileTarget(new Vector2Int((int)tileTarget.x, (int)tileTarget.z), cardData);
    }

    private bool[,] GenerateLOSGrid(int size, out bool[,] simpleLoSGrid)
    {
        int gridSize = 2 * size + 1;
        bool[,] grid = new bool[gridSize, gridSize]; // The middle will be at [size,size]
        simpleLoSGrid = new bool[gridSize, gridSize];
        Vector3 playerEffectivePosition = new Vector3(BattleManager.player.xPos, 0.5f, BattleManager.player.zPos);

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                int realXPosition = BattleManager.player.xPos + (i - size);
                int realZPosition = BattleManager.player.zPos + (j - size);
                if (realXPosition >= 0 && realXPosition < sizeX && realZPosition >= 0 && realZPosition < sizeZ)
                {
                    bool disabledCollider = false;
                    // We want to disable the collider of the wall this tile is on.
                    if (map[realXPosition, realZPosition].tileEntityType == Tile.TileEntityType.wall)
                    {
                        ((Wall)map[realXPosition, realZPosition].GetEntityOnTile()).gameObject.GetComponent<BoxCollider>().enabled = false;
                        disabledCollider = true;
                    }

                    // Now check the collision
                    Vector3 fromPosition = new Vector3(realXPosition, 0.5f, realZPosition);
                    //Debug.Log("Checking from " + fromPosition + " to " + playerEffectivePosition + ", and assigning that to " + i + ", " + j);
                    grid[i, j] = CheckLoS(playerEffectivePosition, fromPosition );
                    simpleLoSGrid[i, j] = CheckSimpleLoS(playerEffectivePosition, fromPosition);

                    if (disabledCollider)
                    {
                        ((Wall)map[realXPosition, realZPosition].GetEntityOnTile()).gameObject.GetComponent<BoxCollider>().enabled = true;
                    }
                }
            }
        }
        return grid;
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
            tarX = UnityEngine.Random.Range(1, sizeX-1);
            tarZ = UnityEngine.Random.Range(1, sizeZ-1);

            if (map[tarX, tarZ].tileEntityType == Tile.TileEntityType.empty)
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
                if (map[i,j].tileEntityType == Tile.TileEntityType.empty)
                {
                    return new Vector2Int(i, j);
                }
            }
        }

        Debug.LogError("BattleGrid--ForceFindEmptyTile():: And we couldn't even find one by looping through. What did you do to the game board!?!?");
        return Vector2Int.one;
    }

    // Used by enemies to check if this is a valid move.
    public bool IsMoveValid(Vector2Int tileLoc, TileEntity mover)
    {
        if (BattleManager.IsVectorNonNeg(tileLoc) && map[tileLoc.x, tileLoc.y].GetPlayerWalkability())
        {
            // Now we need to check if we're moving diagonal.
            int xDir = mover.xPos - tileLoc.x;
            int zDir = mover.zPos - tileLoc.y;

            if (xDir * zDir != 0 && !(map[mover.xPos, tileLoc.y].GetPlayerWalkability() && map[tileLoc.x, mover.zPos].GetPlayerWalkability())) // Both are non-zero
                return false;
            else
                return true;
        }

        return false;
    }
}
