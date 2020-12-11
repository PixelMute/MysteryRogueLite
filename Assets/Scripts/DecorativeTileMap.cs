using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DecorativeTileMap : MonoBehaviour
{
    public Tilemap TileMap;
    public Tile LadderDown;
    public GameObject Torch;
    public GameObject SideTorch;
    public List<(GameObject, TorchDirection)> torches;
    public enum TorchDirection { regular, left, right };
    public static DecorativeTileMap instance;


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
        torches = new List<(GameObject, TorchDirection)>();
    }

    public void PaintStairs(Vector3Int stairsLocation)
    {
        TileMap.SetTile(stairsLocation, LadderDown);
    }

    public void Clear()
    {
        TileMap.ClearAllTiles();
        for (int i = torches.Count - 1; i >= 0; i--)
        {
            Destroy(torches[i].Item1);
        }
        torches.Clear();
    }

    public void InstantiateTorch(int x, int y)
    {
        var torch = Instantiate(Torch, new Vector3(x, .01f, y), Quaternion.Euler(new Vector3(90, 0, 0)), transform);
        torches.Add((torch, TorchDirection.regular));
    }

    public static void SpawnTorch(int x, int y)
    {
        instance.InstantiateTorch(x, y);
    }

    public void InstantiateSideTorch(int x, int y, bool facingRight)
    {
        var sideTorch = Instantiate(SideTorch, new Vector3(x, .01f, y), Quaternion.Euler(new Vector3(90, 0, 0)), transform);
        if (!facingRight)
        {
            sideTorch.GetComponent<SpriteRenderer>().flipX = true;
        }
        torches.Add((sideTorch, facingRight ? TorchDirection.right : TorchDirection.left));
    }

    public static void SpawnSideTorch(int x, int y, bool facingRight)
    {
        instance.InstantiateSideTorch(x, y, facingRight);
    }
}
