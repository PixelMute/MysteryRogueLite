using NesScripts.Controls.PathFind;
using System.Collections.Generic;
using UnityEngine;

public abstract class CreatureBrain : MonoBehaviour
{

    public int visionRange = 10;
    public abstract void StartOfTurn();
    public abstract void ActionPhase();
    public abstract void EndOfTurn();
}

public abstract class EnemyBrain : CreatureBrain
{
    public int shoutRange = 10;
    public int hearingRange = 8;
    public abstract void SummonForHelp(List<Point> pathToFollow);
}

