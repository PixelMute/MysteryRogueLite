using NesScripts.Controls.PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public abstract class CreatureBrain : MonoBehaviour
{
    public abstract void StartOfTurn();
    public abstract void ActionPhase();
    public abstract void EndOfTurn();
}

public abstract class EnemyBrain : CreatureBrain
{
    public abstract void SummonForHelp(EnemyBody genericEnemy, List<Point> pathToFollow);
}

