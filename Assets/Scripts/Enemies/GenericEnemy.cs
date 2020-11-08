using NesScripts.Controls.PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// This script handles generic enemy behavior.
public abstract class GenericEnemy : TileCreature
{
    // Variables
    [HideInInspector] private int health;
    public int maxHealth = 70;
    public int damage = 10;
    [HideInInspector] public int shoutRange = 10;
    [HideInInspector] public int hearingRange = 8;
    public bool isDead = false;

    // Stuff for debugging pathfinding
    protected const bool DEBUGPATHFINDINGMODE = true;
    [HideInInspector] public Color enemyColor;
    [HideInInspector] public float enemyLineY; // so that all the lines are on different y levels.

    // Display
    public GameObject enemyCanvasPrefab;
    public GameObject enemyCanvas; // The actual object, not the prefab
    protected HealthBarScript healthBar;
    protected FadeObjectScript healthBarFade;
    protected FadeObjectScript exclamationPointFade;

    // AI
    public enum EnemyAIState { standing, inCombat, movingToWanderTarget, milling, alerted } // Describes what the enemy is doing right now
    [SerializeField] protected EnemyAIState currentAIState = EnemyAIState.standing;
    protected int turnsLeftToMill = 1; // Number of turns the enemy will spend wandering in a small area
    [SerializeField] protected Vector2Int wanderTarget; // Square the enemy wants to go to
    protected bool isMoving = false;
    protected const bool RECALC_EACH_STEP = false; // Do we want to recalculate pathfinding every step, or only when we get stuck?
    public List<Point> intentPath = new List<Point>();

    // Movement
    protected Vector3 moveTarget; // Where this is currently moving towards
    protected float inverseMovementTime = 6f;

    protected TileEntity engagedTarget = null; // What target this is engaged with.

    public int Health
    {
        get { return health; }
        set { health = value; if (healthBar != null) healthBar.UpdateHealthDisplay(health, maxHealth); }
    }

    public override float GetPathfindingCost()
    {
        return 4f; // Other enemies could wait for this one to move.
    }

    protected void StartMovementTowards(Vector2Int target)
    {
        MoveToPosition(target);
        //Movement.MoveToPosition(target);
        //StartMovementTowards(BattleManager.ConvertVector(target, transform.position.y), target);
    }

    // Enemy decides what they're going to do this turn
    public virtual void ProcessTurn()
    {
        if (isDead)
            return; // The dead don't act.

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
                exclamationPointFade.StartFadeCycle(1, 1.25f);
                InitCombat();
                ShoutForHelp();
            }
        }
        else
        {
            if (currentAIState == EnemyAIState.inCombat || currentAIState == EnemyAIState.alerted) // Did we just get out of a fight?
            {
                // Go to where we last saw the target
                intentPath = FindPathTo(engagedTarget.xPos, engagedTarget.zPos, Pathfinding.DistanceType.NoCornerCutting);
                DisengageTarget();
                currentAIState = EnemyAIState.movingToWanderTarget;
                turnsLeftToMill = 0;
            }
            DecideIdleAction();
        }

    }

    // Tries to get help from other enemies.
    // For each other enemy, see if they are within range.
    // Range is the average of our shoutdistance and their hearing range.
    // If they are in range, they'll wander over here.
    protected void ShoutForHelp()
    {
        Debug.Log("I shout for help! Is anyone out there?");
        // Get the list of enemies.
        foreach (GenericEnemy x in BattleGrid.instance.CurrentFloor.enemies)
        {
            if (x != this)
            {
                // Check straight line distance
                int helpDistance = (x.hearingRange + shoutRange) / 2;
                if (Vector3.Distance(transform.position, x.transform.position) <= helpDistance)
                {
                    // Now check actual distance
                    List<Point> audioPath = x.FindPathTo(xPos, zPos, Pathfinding.DistanceType.NoCornerCutting);
                    if (audioPath.Count <= helpDistance)
                    {
                        Debug.Log("Found somebody to help. Calling " + x.name + " over.");
                        x.SummonForHelp(this, audioPath);
                    }
                }
            }
        }
    }

    // Summons this enemy to start wandering towards another enemy
    public void SummonForHelp(GenericEnemy genericEnemy, List<Point> pathToFollow)
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

    // Checks to see if we've met the condition to engage in combat.
    protected virtual bool InCombatCondition(TileEntity target)
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
                pc.AddEngagedEnemy(this);
            }
        }

    }

    // Performs the enemy's attack action.
    protected abstract void LaunchAttack(Vector2Int target);

    // Things that should happen when this enemy engages a target
    protected abstract void InitCombat();

    // Takes the next step in a list of points and returns the same list, minus the point at index 0 if successful
    protected List<Point> TakeStepInPath(List<Point> path, bool recalcIfFail)
    {
        // Take the next step.
        Point nextStep = path[0];
        Vector2Int nextStepVector = BattleManager.ConvertPoint(nextStep);

        // Is this step valid?
        if (BattleGrid.instance.IsMoveValid(nextStepVector, this))
        {
            // Take this step.
            StartMovementTowards(nextStepVector);
            path.RemoveAt(0);
        }
        else
        {
            if (recalcIfFail)
            {
                // Something is blocking us. Recalculate.
                path = FindPathTo(wanderTarget.x, wanderTarget.y, Pathfinding.DistanceType.NoCornerCutting);
            }
        }

        return path;
    }

    // sets engagedTarget to null and removes ourselves from the list of engaged enemies.
    protected void DisengageTarget()
    {
        if (engagedTarget == null)
            return;

        if (engagedTarget is PlayerController)
        {
            PlayerController pc = (PlayerController)engagedTarget;
            pc.RemoveEngagedEnemy(this);
        }
        else
        {
            Debug.LogError("GenericEnemy--DisengageTarget()::engagedTarget " + engagedTarget.name + " is not a player. You didn't prepare me for this.");
        }
    }

    // We've seen the player. 
    protected abstract void DecideCombatTurn();

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
            intentPath = FindPathTo(wanderTarget.x, wanderTarget.y, Pathfinding.DistanceType.NoCornerCutting);
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

    // Returns a list of points towards a target from where we're standing.
    protected List<Point> FindPathTo(int x, int z, Pathfinding.DistanceType distanceType)
    {
        Point _from = new Point(xPos, zPos);
        Point _to = new Point(x, z);
        return Pathfinding.FindPath(BattleManager.instance.map.walkGrid, _from, _to, distanceType);
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
            Vector2Int tileLoc = new Vector2Int(xDir + xPos, zDir + zPos);

            // Try to move in that direction
            if (BattleGrid.instance.IsMoveValid(tileLoc, this))
            {
                if (intentPath.Count > 0)
                    intentPath.Clear();

                intentPath.Add(new Point(tileLoc.x, tileLoc.y));
            }
        }
    }

    // Initializes the healthbar
    protected HealthBarScript SetUpHealthBar(float yOffset)
    {
        enemyCanvas = Instantiate(enemyCanvasPrefab, transform);
        enemyCanvas.transform.position += new Vector3(0, yOffset, 0);
        return enemyCanvas.GetComponentInChildren<HealthBarScript>();
    }

    // Makes the canvas visible
    public void DisplayHealthBar()
    {
        healthBarFade.StopFade(1f);
        enemyCanvas.SetActive(true);
    }

    // Set speedMult to <= 0 for instant
    public void HideHealthBar(float buffer, float speedMult)
    {
        if (speedMult > 0)
        {
            healthBarFade.StartFadeCycle(buffer, speedMult);
        }
        else
        {
            healthBarFade.StopFade(0f);
            enemyCanvas.SetActive(false);
        }
    }

    protected abstract void DetermineIntent();

    protected abstract void TakeIntent();

    public void HideHealthBar()
    {
        HideHealthBar(-1, -1);
    }

    public override bool GetPlayerWalkable()
    {
        return false;
    }

    public override void TakeDamage(int amount)
    {
        BattleManager.cardResolveStack.AddDamageDealt(amount);
        Health -= amount;
        if (Health <= 0)
            Eliminate(); // KO'd
        else
        {
            healthBarFade.StartFadeCycle(.3f, 1.2f);
        }
    }

    protected void DebugDrawPath(List<Point> path)
    {
        Vector3 fromPosition = transform.position;
        Vector3 toPosition;
        for (int i = 0; i < path.Count; i++)
        {
            toPosition = new Vector3(path[i].x, enemyLineY, path[i].y);
            Debug.DrawLine(fromPosition, toPosition, enemyColor, 2f);
            fromPosition = toPosition;
        }
    }

    // Destroys this gameobject and its children. Also removes it from the battlegrid.
    public void Eliminate()
    {
        isDead = true;

        // Trigger on death effects
        OnDeath();

        // Remove self from the battlegrid
        BattleGrid.instance.ClearTile(new Vector2Int(xPos, zPos));

        // Disengage
        DisengageTarget();

        // Destroy self
        BattleManager.RecursivelyEliminateObject(transform);
    }

    public abstract void OnDeath();

    public override void ApplyStatusEffect(BattleManager.StatusEffectEnum status, int amount)
    {
        Debug.LogError("Generic Enemy -- I was asked to apply a status effect, but that's not implemented.");
    }

    public override int GetStatusEffectValue(BattleManager.StatusEffectEnum status)
    {
        if (statusEffects == null) return 0;

        if (statusEffects.TryGetValue(status, out StatusEffectDataHolder x))
        {
            return x.EffectValue;
        }

        return 0;
    }
}
