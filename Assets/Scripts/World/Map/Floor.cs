using DungeonGenerator;
using DungeonGenerator.core;
using NesScripts.Controls.PathFind;
using System.Collections.Generic;
using System.Linq;
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
    private Tilemap tileMap;
    private Sirpgeon dungeonData;
    private int[] roomsArea; // The number of tiles in each room, ordered in the same order as
    // dungeonData.getRooms();
    private int roomAreaSum = 0;
    public Roguelike.Tile[,] map { get; private set; }
    public int sizeX { get; private set; }
    public int sizeZ { get; private set; }
    public int seed { get; private set; }
    public int FloorNumber { get; private set; }
    public List<EnemyBody> enemies { get; private set; } = new List<EnemyBody>();

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
        CalculateRoomAreas();

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
                    //Color rng = UnityEngine.Random.ColorHSV();
                    //UnityEngine.Debug.DrawLine(new Vector3(i, 0, j), new Vector3(i, 10, j), rng, 100f);
                }
            }
        }

        AssignPlayerLocation();


        int numEnemies = FloorNumber + 1 == 5 ? 25 : 5 + (FloorNumber * 3);
        // Set the enemies locations
        for (int i = 0; i < numEnemies; i++)
        {
            SetEnemyLocation();
        }

        SetStairsLocation();
    }

    // Calculates how many tiles are in each room. Used later for placing objects weighted by room area.
    private void CalculateRoomAreas()
    {
        List<Room> rooms = dungeonData.getRooms();
        int numRooms = rooms.Count();
        roomsArea = new int[numRooms];

        for (int i = 0; i < numRooms; i++)
        {
            Position tl = rooms[i].topLeft();
            Position br = rooms[i].bottomRight();

            //UnityEngine.Debug.Log("In one room. tl.x: " + tl.x() + ", tl.y: " + tl.y() + "; br.x: " + br.x() + ", br.y(): " + br.y());

            for (int j = tl.x(); j < br.x(); j++)
            {
                for (int k = tl.y(); k < br.y(); k++)
                {
                    if (rooms[i].inRoom(new Position(j, k)))
                    {
                        roomsArea[i]++;
                    }
                }
            }
            //UnityEngine.Debug.Log("Room number " + i + " has an area of " + roomsArea[i]);
            roomAreaSum += roomsArea[i];
            /*Color rng = UnityEngine.Random.ColorHSV();

            int testTLX;
            int testTLY;
            int testBRX;
            int testBRY;
            UnityEngine.Debug.DrawLine(new Vector3(tl.x(), 0, tl.y()), new Vector3(tl.x(), 10, tl.y()), rng, 100f);
            UnityEngine.Debug.DrawLine(new Vector3(br.x(), 0, br.y()), new Vector3(br.x(), 10, br.y()), rng, 100f);
            UnityEngine.Debug.DrawLine(new Vector3(br.x(), 0, tl.y()), new Vector3(br.x(), 10, tl.y()), rng, 100f);
            UnityEngine.Debug.DrawLine(new Vector3(tl.x(), 0, br.y()), new Vector3(tl.x(), 10, br.y()), rng, 100f);*/
        }
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
                    case Roguelike.Tile.TileEntityType.enemy:
                        UnityEngine.Debug.Log("Floor spawning enemy");
                        SpawnEnemyAt(i, j);
                        break;
                        //case Roguelike.Tile.TileEntityType.stairsUp:
                        //    var spawnLoc = new Vector2Int(i, j);
                        //    var stairsUp = BattleGrid.instance.SpawnStairsUp(spawnLoc);
                        //    PlaceObjectOn(spawnLoc.x, spawnLoc.y, stairsUp);
                        //    break;
                }

                //// Determine terrain
                //if (map[i, j].tileEntityType != Roguelike.Tile.TileEntityType.wall)
                //{
                //    Tile tile;
                //    if (i % 2 == 0)
                //    {
                //        tile = j % 2 == 0 ? topLeft : topRight;
                //    }
                //    else
                //    {
                //        tile = j % 2 == 0 ? bottomLeft : bottomRight;
                //    }

                //    if (map[i, j].tileTerrainType == Roguelike.Tile.TileTerrainType.stairsDown)
                //    {
                //        var spawnLoc = new Vector2Int(i, j);
                //        var stairsDown = BattleGrid.instance.SpawnStairsDown(spawnLoc);
                //        PlaceTerrainOn(spawnLoc.x, spawnLoc.y, stairsDown);
                //    }

                //    tileMap.SetTile(new Vector3Int(i, j, 0), tile);
                //}
            }
        }

        var painter = new Painter(BattleGrid.instance.DecorativeTileMap);
        painter.Paint(tileMap, map, sizeX, sizeZ);

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
        for (int i = 0; i < 6; i++)
        {
            //Vector2Int spawnLoc = FindTileInRoom(FindTileCondition.empty, FindTileCondition.notPlayersRoom, FindTileCondition.offWall);
            Vector2Int spawnLoc = PickRandomEmptyTile();
            map[spawnLoc.x, spawnLoc.y].tileTerrainType = Roguelike.Tile.TileTerrainType.stairsDown;
        }

        //stairsDownLocation = spawnLoc;
        //if (FloorNumber != 0)
        //{
        //    spawnLoc = PickRandomEmptyTile();
        //    map[spawnLoc.x, spawnLoc.y].tileEntityType = Roguelike.Tile.TileEntityType.stairsUp;
        //    stairsUpLocation = spawnLoc;
        //}
    }

    public void AssignPlayerLocation()
    {
        //Vector2Int spawnLoc = FindTileInRoom(FindTileCondition.empty, FindTileCondition.offWall);
        Vector2Int spawnLoc = PickRandomEmptyTile();
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
                    UnityEngine.Debug.Log("Player Location Found");
                    //flag = true;
                    spawnLocation = new Vector2Int(i, j);
                }
            }
        }

        //var spawnLocation = PickRandomEmptyTile();
        BattleManager.player.EstablishSelf(spawnLocation.x, spawnLocation.y);
        BattleManager.player.transform.position = new Vector3(spawnLocation.x, 0.05f, spawnLocation.y);
    }

    private void SpawnStairs()
    {
        //List<Room> rooms = dungeonData.getRooms();
        //var spawnLoc = new Vector2Int(rooms.Last().bottomRight().x(), rooms.Last().bottomRight().y());
        var spawnLoc = PickRandomEmptyTile();
        var stairsDown = BattleGrid.instance.SpawnStairsDown(spawnLoc);
        PlaceTerrainOn(spawnLoc.x, spawnLoc.y, stairsDown);
        stairsDownLocation = spawnLoc;
        if (FloorNumber != 0)
        {
            //spawnLoc = new Vector2Int(rooms[0].bottomRight().x(), rooms[0].bottomRight().y());
            spawnLoc = PickRandomEmptyTile();
            var stairsUp = BattleGrid.instance.SpawnStairsUp(spawnLoc);
            PlaceTerrainOn(spawnLoc.x, spawnLoc.y, stairsUp);
            stairsUpLocation = spawnLoc;
        }
    }

    public enum FindTileCondition { notPlayersRoom, evenRoomWeighting, offWall, empty };
    public Vector2Int FindTileInRoom(params FindTileCondition[] conds)
    {
        bool forbidPlayer = conds.Contains(FindTileCondition.notPlayersRoom);
        bool weightEven = conds.Contains(FindTileCondition.evenRoomWeighting);
        bool allowEdges = !conds.Contains(FindTileCondition.offWall);
        bool needEmpty = conds.Contains(FindTileCondition.empty);

        List<Room> rooms = dungeonData.getRooms();

        int pickedRoom = -1;
        bool flag = false;
        int tryNum = 0;
        do
        {
            flag = true; // Does this room fit our criteria?
            pickedRoom = -1;
            tryNum++;
            if (weightEven)
            {
                pickedRoom = UnityEngine.Random.Range(0, rooms.Count);
            }
            else
            {
                int randomVal = UnityEngine.Random.Range(0, roomAreaSum);
                do
                {
                    pickedRoom++;
                    randomVal -= roomsArea[pickedRoom];
                } while (randomVal > 0);
            }

            // Have picked a random room. Check if it meets our criteria.
            if (forbidPlayer)
            {
                //UnityEngine.Debug.Log("Player is at " + BattleManager.player.xPos + ", " + BattleManager.player.zPos);
                // UnityEngine.Debug.Log("Room bounded by " + rooms[pickedRoom].topLeft().x() + ", " + rooms[pickedRoom].topLeft().y() + ", and " + rooms[pickedRoom].bottomRight().x() + ", " + rooms[pickedRoom].bottomRight().y());
                // Check if this is the same room as the player.
                Position playerPos = new Position(BattleManager.player.xPos, BattleManager.player.zPos);
                if (rooms[pickedRoom].inRoom(playerPos))
                {
                    //UnityEngine.Debug.Log("True. ");
                    flag = false;
                }
            }
        } while (!flag && tryNum < 20);

        if (tryNum >= 20)
        {
            UnityEngine.Debug.LogWarning("Floor::FindTileInRoom() -- Took 20+ tries to find a room. Going with an arbitrary one.");
        }

        // Now we know which room we're going to. Find a tile in it.
        tryNum = 0;

        do
        {
            flag = true;
            tryNum++;

            int triedX = UnityEngine.Random.Range(rooms[pickedRoom].topLeft().x(), rooms[pickedRoom].bottomRight().x());
            int triedY = UnityEngine.Random.Range(rooms[pickedRoom].topLeft().y(), rooms[pickedRoom].bottomRight().y());

            // Check conditions.
            if (needEmpty)
            {
                if (map[triedX, triedY].tileEntityType != Roguelike.Tile.TileEntityType.empty)
                    flag = false;
            }

            if (!allowEdges && flag) // check if this is next to a wall.
            {
                if (map[triedX + 1, triedY + 1].tileEntityType == Roguelike.Tile.TileEntityType.wall || map[triedX - 1, triedY + 1].tileEntityType == Roguelike.Tile.TileEntityType.wall
                    || map[triedX - 1, triedY - 1].tileEntityType == Roguelike.Tile.TileEntityType.wall || map[triedX + 1, triedY - 1].tileEntityType == Roguelike.Tile.TileEntityType.wall)
                    flag = false;
            }

            if (flag)
            {// This position works.
                return new Vector2Int(triedX, triedY);
            }

        } while (!flag && tryNum < 40);

        UnityEngine.Debug.LogWarning("Floor::FindTileInRoom() -- Could not find suitable tile. Loosening restrictions.");
        UnityEngine.Debug.LogWarning("Room bounded by " + rooms[pickedRoom].topLeft().x() + ", " + rooms[pickedRoom].topLeft().y() + ", and " + rooms[pickedRoom].bottomRight().x() + ", " + rooms[pickedRoom].bottomRight().y() + ". Total rooms: " + rooms.Count);
        //SpawnEnemyAt(rooms[pickedRoom].topLeft().x(), rooms[pickedRoom].topLeft().y(), pickedRoom.ToString());
        //SpawnEnemyAt(rooms[pickedRoom].bottomRight().x(), rooms[pickedRoom].bottomRight().y(), pickedRoom.ToString());
        if (conds.Length <= 1)
        {
            UnityEngine.Debug.LogWarning("Floor::FindTileInRoom() -- Returning a random empty tile.");
            return PickRandomEmptyTile();
        }
        else
        {
            UnityEngine.Debug.LogWarning("Floor::FindTileInRoom() -- Returning an empty tile in a room.");
            return FindTileInRoom(FindTileCondition.empty);
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

    private void SpawnEnemyAt(int x, int z, string name = "")
    {
        var spawnLoc = new Vector2Int(x, z);
        var random = new System.Random();
        var rand = random.Next(1, 4);
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
    private void SpawnWallAt(int x, int z)
    {
        var entityTile = BattleGrid.instance.SpawnWall(x, z);
        PlaceObjectOn(x, z, entityTile);
    }

    /// <summary>
    /// Spawns an item on or around the given tile. Does the backend style stuff, not spawning the gameobject.
    /// This also does not prevent you from spawningan item in a wall. Returns true if placed.
    /// </summary>
    /// <param name="target">Where to try and place the item</param>
    /// <param name="item">What item to place</param>
    /// <param name="bounceBehavior">Use -1 to force place the item on this tile. Otherwise, this is the number of random bounces this item can take before not being placed.
    /// An item will bounce if the targetted tile is taken.</param>
    public bool TryPlaceTileItemOn(Vector2Int target, TileItem item, int bounceBehavior)
    {
        // Make sure we're inside the map.
        if (target.x >= BattleGrid.instance.map.GetLength(0) || target.x < 0 || target.y >= BattleGrid.instance.map.GetLength(1) || target.y < 0)
            return false; // Lost to the either.

        item.xPos = target.x;
        item.zPos = target.y;

        if (map[target.x, target.y].ItemOnTile == null || bounceBehavior <= -1)
        {
            // Target tile is empty, or we need to force spawn item. Spawn the item on it.
            map[target.x, target.y].SetItemOnTile(item);
            return true;
        }
        else if (bounceBehavior == 0)
        { // This item is lost to the ether.
            return false;
        }
        else
        { // Bounce and reduce bounce behavior by 1.
            for (int i = 1; i >= -1; i--) // Up, and to the Right!
            {
                for (int j = 1; j >= -1; j--)
                {
                    if (TryPlaceTileItemOn(new Vector2Int(target.x + i, target.y + j), item, bounceBehavior - 1))
                    {
                        return true; // Found a place for this item.
                    }
                }
            }
            // Didn't find a place.
            return false;
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
}

