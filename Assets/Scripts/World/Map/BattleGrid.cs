using NesScripts.Controls.PathFind;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

// The battlegrid holds data relevant to one floor of a dungeon
public class BattleGrid : MonoBehaviour
{
    [HideInInspector] public static BattleGrid instance;

    public GameObject wallPrefab;
    public Tilemap tileMap;
    public GameObject stairsUp;
    public GameObject stairsDown;
    public FogOfWar FogOfWar;
    public DecorativeTileMap DecorativeTileMap;

    public GameObject terrainHolder;
    public TextMeshProUGUI FloorDisplay;


    // Enemy stuff
    [HideInInspector]
    public GameObject enemyPrefab;

    // Line of Sight
    public LayerMask sightBlockingLayer; // Things that block LOS are on this layer

    // Items
    public GameObject moneyPrefab;

    public Floor CurrentFloor
    {
        get
        {
            return floorManager.CurrentFloor;
        }
    }
    public FloorManager floorManager = new FloorManager();


    public int sizeX
    {
        get
        {
            return CurrentFloor.sizeX;
        }
    }

    public int sizeZ
    {
        get
        {
            return CurrentFloor.sizeZ;
        }
    }

    public WalkableTilesGrid walkGrid
    {
        get
        {
            return CurrentFloor.walkGrid;
        }
    }


    public Roguelike.Tile[,] map
    {
        get
        {
            return CurrentFloor.map;
        }
    }

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

    }

    public void GoDownFloor()
    {
        if (CustomRun.instance.IsLastFloor(floorManager.CurrentFloorNumber))
        {
            SaveGameSystem.instance?.DeleteGame();
            GameOverScreen.PlayerWon();
            return;
        }
        if (!LoadingNewFloor)
        {
            LoadingNewFloor = true;
            AudioManager.PlayNextFloor();
            StartCoroutine(LoadNewFloorCoroutine());
        }
    }

    public bool LoadingNewFloor { get; private set; } = false;
    private IEnumerator LoadNewFloorCoroutine()
    {
        var fader = FindObjectOfType<SceneFader>();
        yield return fader.Fade(SceneFader.FadeDirection.In);                   //Start fading to black
        DecorativeTileMap.Clear();
        floorManager.GoDownFloor();                                             //When screen is black, despawn current floor, generate new floor
        yield return null;
        BattleManager.player.UpdateLOS();                                       //Update player LOS
        FloorDisplay.text = (floorManager.CurrentFloorNumber + 1).ToString();
        FogOfWar.Initialize();
        FogOfWar.ForceUpdate();
        SaveGameSystem.instance?.SaveGame();
        yield return new WaitForSeconds(1f);
        yield return fader.Fade(SceneFader.FadeDirection.Out);               //Fade back in
        LoadingNewFloor = false;
    }

    public void GenerateFirstLevel()
    {
        if (SaveGameSystem.instance?.HasGameToLoad() == true)
        {
            SaveGameSystem.instance.LoadGame();
        }
        else
        {
            SeededRandom.NewRandomSeed();
            Debug.Log($"Random seed: {SeededRandom.Seed}");
            if (CustomRun.instance == null)
            {
                CustomRun.instance = new CustomRun();
            }
            floorManager.GenerateNewFloor();
            //InitFogOfWar();
            FogOfWar.Initialize();

            // Give player the starting bonus event.
            BattleManager.player.puim.RequestEvent(Roguelike.EventDatabase.EventEnum.startingBoon);
        }
    }

    private void OnApplicationQuit()
    {
        SaveGameSystem.instance?.SaveGame();
    }

    //Instantiates wall
    public TileEntity SpawnWall(int x, int z)
    {
        Vector3 spawnLocation = new Vector3(x, transform.position.y + 0.5f, z);
        GameObject go = Instantiate(wallPrefab, spawnLocation, Quaternion.identity, terrainHolder.transform);
        Wall entityTile = go.AddComponent<Wall>();
        return entityTile;
    }

    //Instantiates money and tries to place it. Might not actually get the input spot if the input spot is taken.
    public void SpawnMoneyOnTile(Vector2Int spawnLoc, int amount)
    {
        Vector2Int adjustedSpawnLoc = CurrentFloor.FindSpotForItem(spawnLoc, 2); // It might bounce.
        if (adjustedSpawnLoc.x == -1 && adjustedSpawnLoc.y == -1)
            return; // money was lost to the void.
        GameObject moneyObj = ItemSpawner.SpawnMoney(adjustedSpawnLoc);
        DroppedMoney newMoneyBloodMoney = moneyObj.GetComponent<DroppedMoney>();
        newMoneyBloodMoney.Initialize(amount); // Set how much this is worth
        newMoneyBloodMoney.xPos = spawnLoc.x;
        newMoneyBloodMoney.zPos = spawnLoc.y;
        map[adjustedSpawnLoc.x, adjustedSpawnLoc.y].SetItemOnTile(newMoneyBloodMoney);
    }

    public void ForceSpawnMoney(Vector2Int spawnLoc, int amount)
    {
        GameObject moneyObj = ItemSpawner.SpawnMoney(spawnLoc);
        DroppedMoney newMoneyBloodMoney = moneyObj.GetComponent<DroppedMoney>();
        newMoneyBloodMoney.Initialize(amount); // Set how much this is worth
        newMoneyBloodMoney.xPos = spawnLoc.x;
        newMoneyBloodMoney.zPos = spawnLoc.y;
        map[spawnLoc.x, spawnLoc.y].SetItemOnTile(newMoneyBloodMoney);
    }

    // Picks a random empty tile out of the map.
    public Vector2Int PickRandomEmptyTile()
    {
        return new Vector2Int(0, 0);
    }

    public void DestroyGameObject(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    // Moves the given object from wherever it is to the given location on the battlegrid.
    public void MoveObjectTo(Vector2Int tar, TileEntity obj)
    {
        CurrentFloor.MoveObjectTo(tar, obj);
    }

    // Removes everything on given tile
    public void ClearTile(Vector2Int target)
    {
        if (CurrentFloor.map[target.x, target.y].tileEntityType == Roguelike.Tile.TileEntityType.enemy)
        {
            // Remove this from the list of enemies.
            CurrentFloor.enemies.Remove((EnemyBody)CurrentFloor.map[target.x, target.y].GetEntityOnTile());
        }
        CurrentFloor.map[target.x, target.y].SetEntityOnTile(null);
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
    public bool[,] RecalculateLOS(int size, Vector3 playerTarget, out bool[,] simpleLoSGrid)
    {
        bool[,] LoSGrid = GenerateLOSGrid(size, playerTarget, out simpleLoSGrid);
        return LoSGrid;
    }

    public enum AcceptableTileTargetFailReasons { success, mustTargetCreature };
    // Returns true if this tile is an acceptable target.
    // Only checks conditions POST-HIGHLIGHTING check
    public AcceptableTileTargetFailReasons AcceptableTileTarget(Vector2Int tileTarget, Card cardData)
    {
        bool needsToTargetCreature = false;
        foreach (PlayCondition x in cardData.Range.PlayConditions)
        {
            if (x == PlayCondition.mustHitCreature)
                needsToTargetCreature = true;
        }

        var targetedTile = CurrentFloor.map[tileTarget.x, tileTarget.y];

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
    public int StrikeTile(Vector2Int target, Vector2Int locationOfAttack, int damage)
    {
        Debug.Log("Attacking tile " + target + " for " + damage + " damage.");
        var targetTile = CurrentFloor.map[target.x, target.y];
        if (targetTile.tileEntityType == Roguelike.Tile.TileEntityType.empty)
        {
            return 0;
        }

        return targetTile.GetEntityOnTile().TakeDamage(locationOfAttack, damage);
    }

    /// <summary>
    /// Applies a status effect to the entity on the specified tile.
    /// </summary>
    /// <param name="target">A Vector2Int specifying which tile to target</param>
    /// <param name="status">Enum located in BattleManager that corresponds to which status effect you want to apply</param>
    /// <param name="power">How much status to apply</param>
    /// <returns>True if there was something on this tile.</returns>
    public bool ApplyStatusEffectOnTile(Vector2Int target, BattleManager.StatusEffectEnum status, int power)
    {
        Debug.Log("Applying status effect " + status.ToString() + " on tile " + target + ", with power " + power);
        Roguelike.Tile targetTile = map[target.x, target.y];
        TileCreature tarCreature = targetTile.GetEntityOnTile() as TileCreature;
        if (tarCreature != null)
        {
            tarCreature.ApplyStatusEffect(status, power);
            return true;
        }
        return false;
    }

    public AcceptableTileTargetFailReasons AcceptableTileTarget(Vector3 tileTarget, Card cardData)
    {
        return AcceptableTileTarget(new Vector2Int((int)tileTarget.x, (int)tileTarget.z), cardData);
    }

    private bool[,] GenerateLOSGrid(int size, Vector3 location, out bool[,] simpleLoSGrid)
    {
        int gridSize = 2 * size + 1;
        bool[,] grid = new bool[gridSize, gridSize]; // The middle will be at [size,size]
        simpleLoSGrid = new bool[gridSize, gridSize];
        location = new Vector3((int)location.x, (int)location.y, (int)location.z);

        for (int i = 0; i < gridSize; i++)
        {
            for (int j = 0; j < gridSize; j++)
            {
                int realXPosition = (int)location.x + (i - size);
                int realZPosition = (int)location.z + (j - size);
                if (realXPosition >= 0 && realXPosition < CurrentFloor.sizeX && realZPosition >= 0 && realZPosition < CurrentFloor.sizeZ)
                {
                    bool disabledCollider = false;
                    // We want to disable the collider of the wall this tile is on.
                    if (CurrentFloor.map[realXPosition, realZPosition].tileEntityType == Roguelike.Tile.TileEntityType.wall)
                    {
                        Wall wall = (Wall)CurrentFloor.map[realXPosition, realZPosition].GetEntityOnTile();
                        (wall).gameObject.GetComponent<BoxCollider>().enabled = false;
                        disabledCollider = true;
                    }

                    // Now check the collision
                    Vector3 fromPosition = new Vector3(realXPosition, 0.5f, realZPosition);
                    //Debug.Log("Checking from " + fromPosition + " to " + playerEffectivePosition + ", and assigning that to " + i + ", " + j);
                    grid[i, j] = CheckLoS(location, fromPosition);
                    simpleLoSGrid[i, j] = CheckSimpleLoS(location, fromPosition);

                    if (disabledCollider)
                    {
                        ((Wall)CurrentFloor.map[realXPosition, realZPosition].GetEntityOnTile()).gameObject.GetComponent<BoxCollider>().enabled = true;
                    }
                }
            }
        }
        return grid;
    }

    // Used by enemies to check if this is a valid move.
    public bool IsMoveValid(Vector2Int tileLoc, TileEntity mover)
    {
        if (BattleManager.IsVectorNonNeg(tileLoc) && CurrentFloor.map[tileLoc.x, tileLoc.y].GetPlayerWalkability())
        {
            // Now we need to check if we're moving diagonal.
            int xDir = mover.xPos - tileLoc.x;
            int zDir = mover.zPos - tileLoc.y;

            if (CurrentFloor.Level is BossLevel)
            {
                var bossRoom = ((BossLevel)CurrentFloor.Level).BossRoom;
                if (bossRoom != null && bossRoom.IsInsideRoom(tileLoc))
                {
                    if (!bossRoom.IsEntityAllowedIn(mover))
                    {
                        return false;
                    }
                }
            }


            if (xDir * zDir != 0 && !(CurrentFloor.map[mover.xPos, tileLoc.y].GetPlayerWalkability() && CurrentFloor.map[tileLoc.x, mover.zPos].GetPlayerWalkability())) // Both are non-zero
                return false;
            else
                return true;
        }

        return false;
    }
}
