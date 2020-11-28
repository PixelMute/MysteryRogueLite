using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FogOfWar : MonoBehaviour
{
    public int NumExtraTiles = 20;
    public Tile NotVisitedTile;
    public Tile VisitedTile;
    public Tile VisibleTile;
    public Tilemap TileMap;
    public int SizeX { get; private set; }
    public int SizeZ { get; private set; }
    public Vector2Int PrevPlayerPosition { get; private set; }

    public List<HideInFog> HiddenElements;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    //void Update()
    //{
    //    var playerPosition = BattleManager.ConvertVector(BattleManager.player.transform.position);
    //    if (PrevPlayerPosition != playerPosition)
    //    {
    //        UpdateFogOfWar(playerPosition, BattleManager.player.LoSGrid, BattleManager.player.visionRange);
    //    }

    //    PrevPlayerPosition = playerPosition;
    //}

    public void ForceUpdate()
    {
        var playerPosition = BattleManager.ConvertVector(BattleManager.player.transform.position);
        BattleManager.player.UpdateLOS();
        UpdateFogOfWar(playerPosition, BattleManager.player.LoSGrid, BattleManager.player.visionRange);
        PrevPlayerPosition = playerPosition;
    }

    public void Initialize()
    {
        SizeX = BattleGrid.instance.CurrentFloor.sizeX + NumExtraTiles;
        SizeZ = BattleGrid.instance.CurrentFloor.sizeZ + NumExtraTiles;
        for (int i = -NumExtraTiles / 2; i < SizeX - (NumExtraTiles / 2); i++)
        {
            for (int j = -NumExtraTiles / 2; j < SizeZ - (NumExtraTiles / 2); j++)
            {
                TileMap.SetTile(new Vector3Int(i, j, 0), NotVisitedTile);
            }
        }
        FindElementsToHide();
    }

    private void UpdateFogOfWar(Vector2Int targetLocation, bool[,] losGrid, int visionRange)
    {
        int losX = losGrid.GetLength(0);
        int losY = losGrid.GetLength(1);
        var tilesToRecheck = new List<Vector3Int>();
        for (int i = -1; i <= losX; i++)
        {
            for (int j = -1; j <= losY; j++)
            {
                int x = targetLocation.x + i - (losX / 2);
                int y = targetLocation.y + j - (losY / 2);
                if (i == -1 || j == -1 || i == losX || j == losY)
                {
                    //If this tile was visible but now its not
                    if (TileMap.GetColor(new Vector3Int(x, y, 0)).a < VisitedTile.color.a)
                    {
                        TileMap.SetTile(new Vector3Int(x, y, 0), VisitedTile);
                    }
                    continue;
                }

                if (Vector2Int.Distance(targetLocation, new Vector2Int(x, y)) <= visionRange)
                {
                    if (losGrid[i, j])
                    {
                        TileMap.SetTile(new Vector3Int(x, y, 0), VisibleTile);
                        continue;
                    }
                }
                //If this tile was visible but now its not
                if (TileMap.GetColor(new Vector3Int(x, y, 0)).a < VisitedTile.color.a)
                {
                    TileMap.SetTile(new Vector3Int(x, y, 0), VisitedTile);
                }

            }
        }

        RecheckTiles(BattleGrid.instance.CurrentFloor.Level.Corners);

        HideHiddenElements();
    }

    //Hide the entites that are supposed to be hidden in the fog
    private void HideHiddenElements()
    {
        foreach (var hidden in HiddenElements)
        {
            if (hidden != null)
            {
                var location = new Vector3Int((int)hidden.transform.position.x, (int)hidden.transform.position.z, 0);
                if (TileMap.GetColor(location).a == 0)
                {
                    hidden.Show();
                }
                else
                {
                    hidden.Hide();
                }
            }
        }
    }

    /// <summary>
    /// Recheck certain tiles to see if they are corners. If they are corners then they should be visible
    /// </summary>
    /// <param name="tilesToRecheck"></param>
    private void RecheckTiles(List<Vector2Int> tilesToRecheck)
    {
        foreach (var tile in tilesToRecheck)
        {
            if (tile.x >= 0 && tile.y >= 0 && tile.x < BattleGrid.instance.CurrentFloor.sizeX && tile.y < BattleGrid.instance.CurrentFloor.sizeZ)
            {
                if (BattleGrid.instance.CurrentFloor.map[tile.x, tile.y].tileEntityType == Roguelike.Tile.TileEntityType.wall)
                {
                    int count = 0;
                    if (TileMap.GetColor(new Vector3Int(tile.x - 1, tile.y, 0)).a == 0)
                    {
                        count++;
                    }
                    if (TileMap.GetColor(new Vector3Int(tile.x + 1, tile.y, 0)).a == 0)
                    {
                        count++;
                    }
                    if (TileMap.GetColor(new Vector3Int(tile.x, tile.y - 1, 0)).a == 0)
                    {
                        count++;
                    }
                    if (TileMap.GetColor(new Vector3Int(tile.x, tile.y + 1, 0)).a == 0)
                    {
                        count++;
                    }
                    if (count == 2)
                    {
                        TileMap.SetTile(new Vector3Int(tile.x, tile.y, 0), VisibleTile);
                    }
                    else
                    {

                    }
                }
            }
        }
    }

    private void FindElementsToHide()
    {
        HiddenElements = Object.FindObjectsOfType<HideInFog>().ToList();
    }
}
