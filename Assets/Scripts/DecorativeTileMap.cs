﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DecorativeTileMap : MonoBehaviour
{
    public Tilemap TileMap;
    public Tile LadderDown;
    public GameObject Torch;
    public GameObject SideTorch;
    private List<GameObject> torches;

    public void Awake()
    {
        torches = new List<GameObject>();
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
            Destroy(torches[i]);
        }
    }

    public void SpawnTorch(int x, int y)
    {
        torches.Add(Instantiate(Torch, new Vector3(x, .01f, y), Quaternion.Euler(new Vector3(90, 0, 0)), transform));
    }

    public void SpawnSideTorch(int x, int y, bool facingRight)
    {
        var sideTorch = Instantiate(SideTorch, new Vector3(x, .01f, y), Quaternion.Euler(new Vector3(90, 0, 0)), transform);
        if (!facingRight)
        {
            sideTorch.GetComponent<SpriteRenderer>().flipX = true;
        }
        torches.Add(sideTorch);
    }
}
