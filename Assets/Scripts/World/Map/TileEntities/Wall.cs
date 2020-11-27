using UnityEngine;

public class Wall : TileEntity
{
    //public GameObject wall; // The physical wall this script coresponds to.

    public override float GetPathfindingCost()
    {
        return 0f;
    }

    public override bool GetPlayerWalkable()
    {
        return false;
    }

    public override int TakeDamage(Vector2Int locationOfAttack, int amount)
    {
        Debug.LogError("Wall--TakeDamage(" + amount + "):: I. Wall. ... You. No. Implement. Damage. For. Walls. Yet.");
        return 0;
    }
}
