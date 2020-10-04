using System.Collections;
using System.Collections.Generic;
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

    public override void TakeDamage(int amount)
    {
        Debug.LogError("Wall--TakeDamage(" + amount + "):: I. Wall. ... You. No. Implement. Damage. For. Walls. Yet.");
    }
}
