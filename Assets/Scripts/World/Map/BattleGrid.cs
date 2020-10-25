using DungeonGenerator;
using DungeonGenerator.core;
using FoW;
using NesScripts.Controls.PathFind;
using Roguelike;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
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

    public GameObject terrainHolder;
    public TextMeshProUGUI FloorDisplay;


    // Enemy stuff
    [HideInInspector]
    public GameObject enemyPrefab;

    // Line of Sight
    public LayerMask sightBlockingLayer; // Things that block LOS are on this layer

    // Fog of War
    FogOfWarTeam fogOfWar;
    FogOfWarUnit playerReveal;

    public Floor CurrentFloor
    {
        get
        {
            return floorManager.CurrentFloor;
        }
    }
    private FloorManager floorManager = new FloorManager();


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
        if (floorManager.CurrentFloorNumber + 1 == 5)
        {
            GameOverScreen.PlayerWon();
            return;
        }
        if (!LoadingNewFloor)
        {
            LoadingNewFloor = true;
            StartCoroutine(LoadNewFloorCoroutine());
        }
    }

    private bool LoadingNewFloor = false;
    private IEnumerator LoadNewFloorCoroutine()
    {
        var fader = FindObjectOfType<SceneFader>();
        yield return fader.Fade(SceneFader.FadeDirection.In);                   //Start fading to black
        floorManager.GoDownFloor();                                             //When screen is black, despawn current floor, generate new floor
        SetFogOfWarBounds(CurrentFloor.sizeX, CurrentFloor.sizeZ);              //Update fog of war
        BattleManager.player.UpdateLOS();                                       //Update player LOS
        FloorDisplay.text = (floorManager.CurrentFloorNumber + 1).ToString();
        yield return new WaitForSeconds(1f);
        fogOfWar.ManualUpdate(1f);
        yield return fader.Fade(SceneFader.FadeDirection.Out);               //Fade back in
        LoadingNewFloor = false;
        BattleManager.currentTurn = BattleManager.TurnPhase.player;
    }

    public void GenerateFirstLevel()
    {
        floorManager.GenerateNewFloor();
        InitFogOfWar();
    }

    private void InitFogOfWar()
    {
        fogOfWar = GameObject.Find("FogOfWarHolder").GetComponent<FogOfWarTeam>();
        playerReveal = BattleManager.player.gameObject.GetComponentInChildren<FogOfWarUnit>();
        playerReveal.transform.SetParent(null);
        playerReveal.transform.position = BattleManager.player.transform.position;
        SetFogOfWarBounds(CurrentFloor.sizeX, CurrentFloor.sizeZ);
    }

    //Instantiates wall
    public TileEntity SpawnWall(int x, int z)
    {
        Vector3 spawnLocation = new Vector3(x, transform.position.y + 0.5f, z);
        GameObject go = Instantiate(wallPrefab, spawnLocation, Quaternion.identity, terrainHolder.transform);
        Wall entityTile = go.AddComponent<Wall>();
        return entityTile;
    }

    //Instantiates enemy
    public GenericEnemy SpawnEnemy(Vector2Int spawnLoc)
    {
        GameObject enemyObj = Instantiate(enemyPrefab, BattleManager.ConvertVector(spawnLoc, transform.position.y + 0.05f), Quaternion.identity, transform);
        GenericEnemy newEnemy = enemyObj.GetComponent<GenericEnemy>();
        return newEnemy;
    }

    public TileTerrain SpawnStairsUp(Vector2Int spawnLoc)
    {
        var newObj = Instantiate(stairsUp, BattleManager.ConvertVector(spawnLoc, transform.position.y + 0.05f), stairsUp.transform.rotation);
        return newObj.GetComponent<Stairs>();
    }

    public TileTerrain SpawnStairsDown(Vector2Int spawnLoc)
    {
        var newObj = Instantiate(stairsDown, BattleManager.ConvertVector(spawnLoc, transform.position.y + 0.05f), stairsDown.transform.rotation);
        return newObj.GetComponent<Stairs>();
    }

    // Picks a random empty tile out of the map.
    public Vector2Int PickRandomEmptyTile()
    {
        return CurrentFloor.PickRandomEmptyTile();
    }

    public void DestroyGameObject(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }

    // Sets up the boundary for fog of war based on the map size.
    private void SetFogOfWarBounds(int sizeX, int sizeZ)
    {
        fogOfWar.mapResolution = new Vector2Int(sizeX, sizeZ);
        fogOfWar.mapSize = Mathf.Max(sizeX, sizeZ);
        fogOfWar.mapOffset = new Vector2(sizeX / 2, sizeZ / 2);
        fogOfWar.Reinitialize();
    }

    internal void ProcessEnemyTurn()
    {
        List<GenericEnemy> enemiesLeftToAct = new List<GenericEnemy>();
        // Add all enemies to this list.
        enemiesLeftToAct.AddRange(CurrentFloor.enemies);

        while (enemiesLeftToAct.Count > 0)
        {
            enemiesLeftToAct[0].ProcessTurn();
            enemiesLeftToAct.RemoveAt(0);
        }
    }

    // Moves the given object from wherever it is to the given location on the battlegrid.
    public void MoveObjectTo(Vector2Int tar, TileEntity obj)
    {
        CurrentFloor.MoveObjectTo(tar, obj);
    }

    /*public void MoveObjectTo(Vector3 target, TileEntity obj)
    {
        MoveObjectTo((int)target.x, (int)target.z, obj);
    }*/

    // Removes everything on given tile
    public void ClearTile(Vector2Int target)
    {
        if (CurrentFloor.map[target.x, target.y].tileEntityType == Roguelike.Tile.TileEntityType.enemy)
        {
            // Remove this from the list of enemies.
            CurrentFloor.enemies.Remove((GenericEnemy)CurrentFloor.map[target.x, target.y].GetEntityOnTile());
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
    public bool[,] RecalculateLOS(int size, out bool[,] simpleLoSGrid)
    {
        bool[,] LoSGrid = GenerateLOSGrid(size, out simpleLoSGrid);
        Debug.Log("Updating fog of war");
        playerReveal.transform.position = BattleManager.player.moveTarget;
        fogOfWar.ManualUpdate(1f);
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
    public void StrikeTile(Vector2Int target, int damage)
    {
        Debug.Log("Attacking tile " + target + " for " + damage + " damage.");
        var targetTile = CurrentFloor.map[target.x, target.y];
        if (targetTile.tileEntityType == Roguelike.Tile.TileEntityType.empty)
        {
            return;
        }

        targetTile.GetEntityOnTile().TakeDamage(damage);
    }

    /// <summary>
    /// Applies a status effect to the entity on the specified tile.
    /// </summary>
    /// <param name="target">A Vector2Int specifying which tile to target</param>
    /// <param name="status">Enum located in BattleManager that corresponds to which status effect you want to apply</param>
    /// <param name="power">How much status to apply</param>
    public void ApplyStatusEffectOnTile(Vector2Int target, BattleManager.StatusEffectEnum status, int power)
    {
        Debug.Log("Applying status effect " + status.ToString() + " on tile " + target + ", with power " + power);
        Roguelike.Tile targetTile = map[target.x, target.y];
        TileCreature tarCreature = targetTile.GetEntityOnTile() as TileCreature;
        if (tarCreature != null)
        {
            tarCreature.ApplyStatusEffect(status, power);
        }
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
                    grid[i, j] = CheckLoS(playerEffectivePosition, fromPosition);
                    simpleLoSGrid[i, j] = CheckSimpleLoS(playerEffectivePosition, fromPosition);

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

            if (xDir * zDir != 0 && !(CurrentFloor.map[mover.xPos, tileLoc.y].GetPlayerWalkability() && CurrentFloor.map[tileLoc.x, mover.zPos].GetPlayerWalkability())) // Both are non-zero
                return false;
            else
                return true;
        }

        return false;
    }
}
