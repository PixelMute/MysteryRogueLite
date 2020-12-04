using UnityEngine;

// A TileEntity is something that exists on the BattleGrid.
// Includes walls, enemies, the player...
public abstract class TileEntity : MonoBehaviour
{
    public int xPos;
    public int zPos;

    public abstract float GetPathfindingCost(); // Returns how hard it is for this to be moved through. Used for pathfinding. 0 = unpassable.
    public abstract bool GetPlayerWalkable(); // Returns wether the player can walk on this.

    // Sets up position and tileEntityType
    public void EstablishSelf(int x, int z)
    {
        xPos = x;
        zPos = z;
        BattleGrid.instance.map[x, z].SetEntityOnTile(this);
    }

    // Returns how much damage they took.
    public abstract int TakeDamage(Vector2Int locationOfAttck, int amount);
}
