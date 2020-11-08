using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Roguelike;

public abstract class TileTerrain : MonoBehaviour
{
    public abstract float GetPathfindingCost(); // An multiplicative bonus to the cost to walk over this.
    public abstract bool GetPlayerWalkable(); // Returns wether the player can walk on this.

    public abstract Tile.TileTerrainType GetTerrainType();
}
