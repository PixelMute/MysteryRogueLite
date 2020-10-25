using Roguelike;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Stairs : TileTerrain
{
    public bool IsUp;

    public override float GetPathfindingCost()
    {
        return 1f;  //Assuming 1f is able to be walked through
    }

    public override bool GetPlayerWalkable()
    {
        return true;
    }

    public override Tile.TileTerrainType GetTerrainType()
    {
        if (IsUp)
            return Tile.TileTerrainType.stairsUp;
        else
            return Tile.TileTerrainType.stairsUp;
    }
}

