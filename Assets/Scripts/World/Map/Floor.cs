using DungeonGenerator;
using NesScripts.Controls.PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FloorManager
{
    public const int WinningFloorNumber = 2;
    public int CurrentFloorNumber { get; private set; } = 0;
    public List<Floor> AllFloors { get; private set; } = new List<Floor>();
    public Floor CurrentFloor { get; private set; }

    public void GenerateNewFloor()
    {
        Painter.LoadTiles();
        //CurrentFloor = GenerateRandomFloor(CurrentFloorNumber);
        Level level;
        BattleGrid.instance.tileMap.ClearAllTiles();
        BattleGrid.instance.DecorativeTileMap.Clear();
        if (CurrentFloorNumber == WinningFloorNumber - 1)
        {
            level = new BossLevel(BattleGrid.instance.tileMap, BattleGrid.instance.DecorativeTileMap);
        }
        else
        {
            level = new Level(BattleGrid.instance.tileMap, BattleGrid.instance.DecorativeTileMap);
        }
        CurrentFloor = new Floor(level, CurrentFloorNumber, 0);
        AllFloors.Append(CurrentFloor);
        CurrentFloor.BuildFloor();
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
    public Floor GenerateRandomFloor(int floorNumber, int x = 40, int y = 40, int seed = 0)
    {
        //Allows us to create the exact same floor if we know the seed
        if (seed == 0)
        {
            seed = UnityEngine.Random.Range(-100000, 100000);
        }

        var dungeonData = new Sirpgeon(x, y, seed, DungeonGenerator.gen.hints.GenBorder.ODD, DungeonGenerator.gen.hints.GenDensity.NORMAL,
            DungeonGenerator.gen.hints.GenRoomShape.BOX_AMALGAM, DungeonGenerator.gen.hints.GenRoomSize.HUGE,
            DungeonGenerator.gen.hints.GenMazeType.WILSON, DungeonGenerator.gen.hints.GenPathing.ROOM_TO_ROOM,
            DungeonGenerator.gen.hints.GenEgress.RANDOM_PLACEMENT, 0, 1);
        return new Floor(dungeonData, floorNumber, seed);
    }
}

public class Floor
{
    public Level Level { get; set; }
    public Roguelike.Tile[,] map { get; set; }
    public int sizeX { get; private set; }
    public int sizeZ { get; private set; }
    public int seed { get; private set; }
    public int FloorNumber { get; private set; }
    public List<EnemyBody> enemies { get; private set; } = new List<EnemyBody>();
    public List<Trap> traps { get; private set; } = new List<Trap>();

    // Pathfinding
    private float[,] walkCostsMap = null; // This is the cost of that tile. 0 = impassable.
    public WalkableTilesGrid walkGrid = null;

    public Floor(Sirpgeon dungeonData, int floorNumber, int seed)
    {
        //this.dungeonData = dungeonData;
        //tileMap = BattleGrid.instance.tileMap;
        this.seed = seed;
        FloorNumber = floorNumber;
    }

    public Floor(Level level, int floorNumber, int seed)
    {
        Level = level;
        FloorNumber = floorNumber;
        this.seed = seed;
    }

    //Builds the floor from the dungeon data
    public void BuildFloor()
    {
        var now = DateTime.Now;
        Level.Build();
        var end = DateTime.Now;
        Debug.Log($"It took {(end - now).TotalSeconds} to generate the level");
        sizeX = Level.Bounds.Width;
        sizeZ = Level.Bounds.Height;
        map = new Roguelike.Tile[sizeX, sizeZ];
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                map[i, j] = new Roguelike.Tile();
            }
        }
        Level.Paint();
        ConstructTiles();

        AssignPlayerLocation();

        SetStairsLocation();
    }

    private void ConstructTiles()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                var tile = Level.Terrain.GetTile(new Vector3Int(i, j, 0));
                if (IsTileAWall(tile))
                {
                    map[i, j].tileEntityType = Roguelike.Tile.TileEntityType.wall;
                }
            }
        }
    }

    private bool IsTileAWall(TileBase tile)
    {
        return tile != null && (tile.name.Contains("Wall") || tile.name.Contains("Corner") || tile.name.Contains("Turn"));
    }

    public void InstantiateFloor()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                switch (map[i, j].tileEntityType)
                {
                    case Roguelike.Tile.TileEntityType.wall:
                        SpawnWallAt(i, j);
                        break;
                }
            }
        }

        //Generates the map twice. Not efficient but lazy and it works
        GenerateWalkableMap();

        SpawnEnemies();

        PlaceTraps();

        PlacePlayerInDungeon();

        GenerateWalkableMap();
    }

    private void PlaceTraps()
    {
        traps = Level.GetRequiredTraps();
        var spawnLocations = Level.GetPossibleSpawnLocations();

        foreach (var trap in traps)
        {
            var trapLocation = BattleManager.ConvertVector(trap.transform.position);
            spawnLocations.Remove(trapLocation);
            PlaceTerrainOn(trapLocation.x, trapLocation.y, trap);
        }
        var numTraps = GetNumberOfTrapsForFloor();
        while (traps.Count < numTraps && spawnLocations.Count > 0)
        {
            var spawnLoc = spawnLocations.PickRandom();
            spawnLocations.Remove(spawnLoc);
            var trap = EnemySpawner.SpawnSpikes(spawnLoc).GetComponent<Spikes>();
            traps.Add(trap);
            PlaceTerrainOn(spawnLoc.x, spawnLoc.y, trap);
            if (Random.RandBool(.5f))
            {
                trap.MakeInvisible();
            }
        }
    }

    private int GetNumberOfTrapsForFloor()
    {
        return FloorNumber + 100;
    }

    private void SpawnEnemies()
    {
        var numEnemies = GetNumberOfEnemiesForFloor();
        enemies = Level.GetRequiredEnemies();
        var spawnLocations = Level.GetPossibleSpawnLocations();
        foreach (var enemy in enemies)
        {
            var enemyLocation = BattleManager.ConvertVector(enemy.transform.position);
            spawnLocations.Remove(enemyLocation);
            //If this is the boss
            if (enemy.AI is BossBrain)
            {
                PlaceBoss(enemy);
            }
            else
            {
                map[enemyLocation.x, enemyLocation.y].tileEntityType = Roguelike.Tile.TileEntityType.enemy;
                PlaceObjectOn(enemyLocation.x, enemyLocation.y, enemy);
            }
        }
        while (enemies.Count < numEnemies && spawnLocations.Count > 0)
        {
            var location = spawnLocations.PickRandom();
            spawnLocations.Remove(location);
            map[location.x, location.y].tileEntityType = Roguelike.Tile.TileEntityType.enemy;
            SpawnEnemyAt(location.x, location.y);
        }
    }

    public void PlaceBoss(EnemyBody boss)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                var x = boss.xPos + i;
                var z = boss.zPos + j;
                map[x, z].tileEntityType = Roguelike.Tile.TileEntityType.boss;
                walkCostsMap[x, z] = map[x, z].GetPathfindingCost();
            }
        }
        walkGrid.UpdateGrid(walkCostsMap);
    }

    private int GetNumberOfEnemiesForFloor()
    {
        return FloorNumber + 4;
    }

    public void DespawnFloor()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                if (map[i, j].tileEntityType != Roguelike.Tile.TileEntityType.player && map[i, j].tileEntityType != Roguelike.Tile.TileEntityType.boss)
                {
                    BattleGrid.instance.DestroyGameObject(map[i, j].GetEntityOnTile()?.gameObject);
                }
            }
        }
        Level.ClearAllTiles();
    }

    private void SetStairsLocation()
    {
        var stairsLocation = Level.Exit.StairsLocation;
        Vector2Int spawnLoc;
        if (stairsLocation == null)
        {
            Debug.LogError("Exit didn't have any valid positions to spawn stairs");
            spawnLoc = PickRandomEmptyTile();
        }
        else
        {
            spawnLoc = stairsLocation.Value;
        }
        map[spawnLoc.x, spawnLoc.y].tileTerrainType = Roguelike.Tile.TileTerrainType.stairsDown;
        //var stairsDown = new Stairs()
        //{
        //    IsUp = false,
        //};
        //PlaceTerrainOn(spawnLoc.x, spawnLoc.y, stairsDown);
    }

    public void AssignPlayerLocation()
    {
        var playerSpawn = Level.Entrance.PlayerSpawn;
        Vector2Int spawnLoc;
        if (playerSpawn == null)
        {
            Debug.LogError("Entrance didn't have any valid positions to spawn");
            spawnLoc = PickRandomEmptyTile();
        }
        else
        {
            spawnLoc = playerSpawn.Value;
        }
        map[spawnLoc.x, spawnLoc.y].tileEntityType = Roguelike.Tile.TileEntityType.player;
        BattleManager.player.xPos = spawnLoc.x;
        BattleManager.player.zPos = spawnLoc.y;
    }

    public void PlacePlayerInDungeon()
    {
        // Find the spot set to player.
        Vector2Int spawnLocation = PickRandomEmptyTile();
        //bool flag = false;

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                if (map[i, j].tileEntityType == Roguelike.Tile.TileEntityType.player)
                {
                    //UnityEngine.Debug.Log("Player Location Found");
                    //flag = true;
                    spawnLocation = new Vector2Int(i, j);
                }
            }
        }

        //var spawnLocation = PickRandomEmptyTile();
        BattleManager.player.EstablishSelf(spawnLocation.x, spawnLocation.y);
        BattleManager.player.transform.position = new Vector3(spawnLocation.x, 0.05f, spawnLocation.y);
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

    private void SpawnEnemyAt(int x, int z, string name = "")
    {
        var spawnLoc = new Vector2Int(x, z);
        var rand = Random.Range(1, 4);
        GameObject newEnemy;
        switch (rand)
        {
            case 1:
                newEnemy = EnemySpawner.SpawnArcher(spawnLoc);
                break;
            case 2:
                newEnemy = EnemySpawner.SpawnBrute(spawnLoc);
                break;
            default:
                newEnemy = EnemySpawner.SpawnBasicMeleeEnemy(spawnLoc);
                break;
        }
        var enemyBody = newEnemy.GetComponent<EnemyBody>();
        enemies.Add(enemyBody);
        enemyBody.name = "EnemyID: " + enemies.Count;
        PlaceObjectOn(spawnLoc.x, spawnLoc.y, enemyBody);
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
    public void SpawnWallAt(int x, int z)
    {
        var entityTile = BattleGrid.instance.SpawnWall(x, z);
        PlaceObjectOn(x, z, entityTile);
    }

    public void DestroyWallAt(int x, int z)
    {
        var entity = map[x, z].GetEntityOnTile();
        if (entity is Wall)
        {
            UnityEngine.Object.Destroy(entity.gameObject);
        }
        map[x, z].SetEntityOnTile(null);
    }

    /// <summary>
    /// Finds a spot to place an item. Does not actually place it.
    /// </summary>
    /// <param name="baseTarget">Where to try and place the item</param>
    /// <param name="item">What item to place</param>
    /// <param name="bounceBehavior">Use -1 to force place the item on this tile. Otherwise, this is the number of random bounces this item can take before not being placed.
    /// An item will bounce if the targetted tile is taken.
    /// Returns (-1,-1) when it doesn't find a spot.</param>
    public Vector2Int FindSpotForItem(Vector2Int baseTarget, int bounceBehavior)
    {
        // Make sure we're inside the map.
        if (baseTarget.x >= BattleGrid.instance.map.GetLength(0) || baseTarget.x < 0 || baseTarget.y >= BattleGrid.instance.map.GetLength(1) || baseTarget.y < 0)
            return new Vector2Int(-1, -1); // Lost to the either.
        if (map[baseTarget.x, baseTarget.y].tileEntityType == Roguelike.Tile.TileEntityType.wall)
            return new Vector2Int(-1, -1); // Can't stick it in a wall.

        //item.xPos = baseTarget.x;
        //item.zPos = baseTarget.y;

        if (map[baseTarget.x, baseTarget.y].ItemOnTile == null || bounceBehavior <= -1)
        {
            // Target tile is empty, or we need to force spawn item. Spawn the item on it.
            //map[baseTarget.x, baseTarget.y].SetItemOnTile(item);
            return baseTarget;
        }
        else if (bounceBehavior == 0)
        { // This item is lost to the ether.
            return new Vector2Int(-1, -1);
        }
        else
        { // Bounce and reduce bounce behavior by 1.
            for (int i = 1; i >= -1; i--) // Up, and to the Right!
            {
                for (int j = 1; j >= -1; j--)
                {
                    Vector2Int recVec = FindSpotForItem(new Vector2Int(baseTarget.x + i, baseTarget.y + j), bounceBehavior - 1);
                    if (recVec.x > -1 && recVec.y > -1)
                    {
                        return recVec; // Found a place for this item.
                    }
                }
            }
            // Didn't find a place.
            return new Vector2Int(-1, -1);
        }
    }

    // Sets obj's internal tracking of position, as well as updates the battlegrid.
    public void PlaceObjectOn(int x, int z, TileEntity obj)
    {
        float oldCost = 0f;
        if (walkCostsMap != null) // Might have to recalculate now that something has moved.
        {
            oldCost = map[x, z].GetPathfindingCost();
        }

        obj.xPos = x;
        obj.zPos = z;
        map[x, z].SetEntityOnTile(obj);


        if (walkCostsMap != null && oldCost != map[x, z].GetPathfindingCost()) // Need to update
        {
            walkCostsMap[x, z] = map[x, z].GetPathfindingCost();
            walkGrid.UpdateGrid(walkCostsMap);
        }
    }

    public void PlaceTerrainOn(int x, int z, TileTerrain ter)
    {
        if (walkCostsMap != null && ter.GetPathfindingCost() != 1f) // Might have to recalculate now that something has moved.
        {
            walkCostsMap[x, z] = map[x, z].GetPathfindingCost();
            walkGrid.UpdateGrid(walkCostsMap);
        }

        ter.xPos = x;
        ter.zPos = z;

        map[x, z].SetTerrainOnTile(ter);

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

    public void ClearBossTiles()
    {
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                if (map[i, j].tileEntityType == Roguelike.Tile.TileEntityType.boss)
                {
                    map[i, j].tileEntityType = Roguelike.Tile.TileEntityType.empty;
                    walkCostsMap[i, j] = map[i, j].GetPathfindingCost();
                }
            }
        }
        walkGrid.UpdateGrid(walkCostsMap);
    }
}

