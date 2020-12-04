// An abstract class that represents one stack of item on the ground.

using UnityEngine;
using Roguelike;

public abstract class TileItem : MonoBehaviour
{
    public int xPos;
    public int zPos;

    // Destroys self
    public void DestroySelf()
    {
        BattleManager.RecursivelyEliminateObject(this.transform);
    }
}
