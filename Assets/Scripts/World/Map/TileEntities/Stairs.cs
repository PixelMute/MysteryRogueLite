using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Stairs : TileEntity
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


    public override void TakeDamage(int amount)
    {
        return;     //Doesn't need to be able to take damage
    }
}

