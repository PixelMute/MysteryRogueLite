using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class Attack : MonoBehaviour
{
    public abstract bool IsTargetInRange(Vector2Int target);

    public abstract void ActivateAttack(Vector2Int target);
}

