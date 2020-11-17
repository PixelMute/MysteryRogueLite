using UnityEngine;
using UnityEngine.Tilemaps;

public class DecorativeTileMap : MonoBehaviour
{
    public Tilemap TileMap;
    public Tile LadderDown;

    public void PaintStairs(Vector3Int stairsLocation)
    {
        TileMap.SetTile(stairsLocation, LadderDown);
    }

    public void Clear()
    {
        TileMap.ClearAllTiles();
    }
}
