using NesScripts.Controls.PathFind;
using System.Collections.Generic;
using UnityEngine;

public class BossBrain : EnemyBrain
{
    public bool Activated = false;

    private EnemyBody Body;

    public void Start()
    {
        Body = GetComponent<EnemyBody>();
    }

    public override void ActionPhase()
    {
        if (Activated)
        {
            var player = BattleManager.player;
            //If close enough to attack, then attack
            if (Body.Attack.IsTargetInRange(BattleManager.ConvertVector(player.transform.position)))
            {
                Body.Animation.Attack();
                if (player.transform.position.x > transform.position.x)
                {
                    Body.Animation.TurnRight();
                }
                else if (player.transform.position.x < transform.position.x)
                {
                    Body.Animation.TurnLeft();
                }
                Body.Attack.ActivateAttack(BattleManager.ConvertVector(player.transform.position));
            }
            //Else move closer to enemy
            else
            {
                List<Point> pathToCombatTarget = Body.FindPathTo(player.xPos, player.zPos, Pathfinding.DistanceType.NoCornerCutting);
                pathToCombatTarget = TakeStepInPath(pathToCombatTarget, false);
            }
        }
    }

    // Takes the next step in a list of points and returns the same list, minus the point at index 0 if successful
    protected List<Point> TakeStepInPath(List<Point> path, bool recalcIfFail)
    {
        // Take the next step.
        Point nextStep = path[0];
        Vector2Int nextStepVector = BattleManager.ConvertPoint(nextStep);

        // Is this step valid?
        if (BattleGrid.instance.IsMoveValid(nextStepVector, Body))
        {
            // Take this step.
            Body.Animation.Move();
            if (nextStepVector.x > transform.position.x)
            {
                Body.Animation.TurnRight();
            }
            else if (nextStepVector.x < transform.position.x)
            {
                Body.Animation.TurnLeft();
            }
            Body.MoveToPosition(nextStepVector);
            path.RemoveAt(0);
        }
        else
        {
            if (recalcIfFail)
            {
                // Something is blocking us. Recalculate.
                path = Body.FindPathTo(BattleManager.player.xPos, BattleManager.player.zPos, Pathfinding.DistanceType.NoCornerCutting);
            }
        }

        return path;
    }


    public override void EndOfTurn()
    {
        return;
    }

    public override void StartOfTurn()
    {
        return;
    }

    public override void SummonForHelp(List<Point> pathToFollow)
    {
        //The boss doesn't respond to calls for help
        return;
    }
}

