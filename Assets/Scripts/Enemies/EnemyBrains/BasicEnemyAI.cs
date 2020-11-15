using NesScripts.Controls.PathFind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class BasicEnemyAI : EnemyBrain
{
    private EnemyBody EnemyBody;
    public void Start()
    {
        EnemyBody = GetComponent<EnemyBody>();
    }


    // AI
    public enum EnemyAIState { standing, inCombat, movingToWanderTarget, milling, alerted } // Describes what the enemy is doing right now
    [SerializeField] protected EnemyAIState currentAIState = EnemyAIState.standing;
    protected int turnsLeftToMill = 1; // Number of turns the enemy will spend wandering in a small area
    [SerializeField] protected Vector2Int wanderTarget; // Square the enemy wants to go to
    protected bool isMoving = false;
    protected const bool RECALC_EACH_STEP = false; // Do we want to recalculate pathfinding every step, or only when we get stuck?
    public List<Point> intentPath = new List<Point>();
    protected TileEntity engagedTarget = null; // What target this is engaged with.

    public int visionRange = 10;

    public override void StartOfTurn()
    {
        return;
    }

    public override void EndOfTurn()
    {
        return;
    }

    public override void ActionPhase()
    {
        StandardEnemyTurn();
    }

    // Are we idle or in combat?
    protected virtual void StandardEnemyTurn()
    {
        // Can we see the player?
        if (InCombatCondition(BattleManager.player))
        {
            EngageTarget(BattleManager.player);
            if (currentAIState == EnemyAIState.inCombat || currentAIState == EnemyAIState.alerted)
            {
                // Take a standard combat action
                currentAIState = EnemyAIState.inCombat;
                DecideCombatTurn();
            }
            else
            {
                // Go to alerted, take no action. But do shout for help.
                currentAIState = EnemyAIState.alerted;
                EnemyBody.EnemyUI.FadeExclaimationPoint();
                EnemyBody.ShoutForHelp();
            }
        }
        else
        {
            if (currentAIState == EnemyAIState.inCombat || currentAIState == EnemyAIState.alerted) // Did we just get out of a fight?
            {
                // Go to where we last saw the target
                intentPath = EnemyBody.FindPathTo(engagedTarget.xPos, engagedTarget.zPos, Pathfinding.DistanceType.NoCornerCutting);
                DisengageTarget();
                currentAIState = EnemyAIState.movingToWanderTarget;
                turnsLeftToMill = 0;
            }
            DecideIdleAction();
        }

    }

    // Checks to see if we've met the condition to engage in combat.
    protected bool InCombatCondition(TileEntity target)
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);
        return (distance <= visionRange && BattleGrid.instance.CheckLoS(transform.position, target.transform.position));
    }

    // Sets the given target as the thing this enemy is attacking.
    protected void EngageTarget(TileEntity target)
    {
        if (target != engagedTarget)
        {
            if (engagedTarget != null)
            {
                // We already have a target. Need to DisengageTarget
                DisengageTarget();
            }

            engagedTarget = target;
            PlayerController pc = target as PlayerController;
            if (pc != null)
            {
                pc.AddEngagedEnemy(EnemyBody);
            }
        }

    }

    // We've seen the player. What should we do?
    // This enemy plans to take a step forward and attack.
    // If it ends its turn within 2 tiles of you, it will attack next turn.
    protected void DecideCombatTurn()
    {
        if (EnemyBody.Attack.IsTargetInRange(BattleManager.ConvertVector(engagedTarget.transform.position)))
        {
            EnemyBody.Attack.ActivateAttack(BattleManager.ConvertVector(engagedTarget.transform.position));
        }

    }

    // Takes the next step in a list of points and returns the same list, minus the point at index 0 if successful
    protected List<Point> TakeStepInPath(List<Point> path, bool recalcIfFail)
    {
        // Take the next step.
        Point nextStep = path[0];
        Vector2Int nextStepVector = BattleManager.ConvertPoint(nextStep);

        // Is this step valid?
        if (BattleGrid.instance.IsMoveValid(nextStepVector, EnemyBody))
        {
            // Take this step.
            EnemyBody.MoveToPosition(nextStepVector);
            path.RemoveAt(0);
        }
        else
        {
            if (recalcIfFail)
            {
                // Something is blocking us. Recalculate.
                path = EnemyBody.FindPathTo(wanderTarget.x, wanderTarget.y, Pathfinding.DistanceType.NoCornerCutting);
            }
        }

        return path;
    }

    // sets engagedTarget to null and removes ourselves from the list of engaged enemies.
    public void DisengageTarget()
    {
        if (engagedTarget == null)
            return;

        if (engagedTarget is PlayerController)
        {
            PlayerController pc = (PlayerController)engagedTarget;
            pc.RemoveEngagedEnemy(EnemyBody);
        }
        else
        {
            Debug.LogError("GenericEnemy--DisengageTarget()::engagedTarget " + engagedTarget.name + " is not a player. You didn't prepare me for this.");
        }
    }

    // Decide what the enemy is going to do in the absense of the player
    protected virtual void DecideIdleAction()
    {
        //Debug.Log("I'm an enemy deciding what to do with my time. Maybe take up art?");
        if (turnsLeftToMill > 0)
        {
            //Debug.Log("I'm going to mill about.");
            MillAbout(); // We've reached the wander target and now should just mill about.
            turnsLeftToMill--;
        }
        else
        {
            //Debug.Log("I'm going to wander somewhere.");
            if (currentAIState != EnemyAIState.movingToWanderTarget) // We don't have a wander target
            {
                //Debug.Log("Picking new wander target.");
                PickNewWanderTarget();
                currentAIState = EnemyAIState.movingToWanderTarget;
            }

            // Do we have a path to the wander target?

            if (intentPath.Count > 0)
            {
                //Debug.Log("Taking a step towards my target");
                intentPath = TakeStepInPath(intentPath, true);
            }
            else
            {
                //Debug.Log("Out of steps. Milling about.");
                currentAIState = EnemyAIState.milling;
                turnsLeftToMill = UnityEngine.Random.Range(4, 12);
            }

        }
    }

    // Picks a new target to wander towards.
    protected void PickNewWanderTarget()
    {
        int tryNum = 0;
        do
        {
            tryNum++;
            wanderTarget = BattleGrid.instance.PickRandomEmptyTile();
            // Now check if we have a path to it.
            intentPath = EnemyBody.FindPathTo(wanderTarget.x, wanderTarget.y, Pathfinding.DistanceType.NoCornerCutting);
        } while (intentPath.Count == 0 && tryNum < 10);

        if (tryNum >= 10)
        {
            // Didn't find anything. Just mill about.
            intentPath.Clear();
        }
        else
        {
            //Debug.Log("AI here. Found a path to " + wanderTarget.x + ", " + wanderTarget.y + " with " + intentPath.Count + " steps.");
        }
    }

    // Wander around in random directions
    protected void MillAbout()
    {
        // Wander around this area.
        if (UnityEngine.Random.Range(0f, 1f) < .1)
        {
            // do nothing.
        }
        else
        {
            // Pick a random direction
            int xDir = UnityEngine.Random.Range(-1, 2);
            int zDir = UnityEngine.Random.Range(-1, 2);
            int xPos = (int)transform.position.x;
            int zPos = (int)transform.position.z;
            Vector2Int tileLoc = new Vector2Int(xDir + xPos, zDir + zPos);

            // Try to move in that direction
            if (BattleGrid.instance.IsMoveValid(tileLoc, EnemyBody))
            {
                if (intentPath.Count > 0)
                    intentPath.Clear();

                intentPath.Add(new Point(tileLoc.x, tileLoc.y));
            }
        }
    }

    public override void SummonForHelp(EnemyBody genericEnemy, List<Point> pathToFollow)
    {
        // If we're currently in combat, sorry, but I have bigger things to worry about.
        if (currentAIState == EnemyAIState.alerted || currentAIState == EnemyAIState.inCombat)
        {
            Debug.Log("I've been summoned to help, but I'm currently in combat.");
            return;
        }
        else
        {
            Debug.Log("I've been summoned to help and now I'm going over there.");
            turnsLeftToMill = 0;
            currentAIState = EnemyAIState.movingToWanderTarget;
            intentPath = pathToFollow;
        }
    }
}

