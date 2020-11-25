using NesScripts.Controls.PathFind;
using System.Collections.Generic;
using UnityEngine;

// This script handles generic enemy behavior.
//[RequireComponent(typeof(EnemyUI))]
//[RequireComponent(typeof(Health))]
//[RequireComponent(typeof(Attack))]
public class EnemyBody : TileCreature
{
    public EnemyUI EnemyUI { get; private set; }
    public Health Health { get; private set; }
    public Attack Attack { get; private set; }
    public EnemyBrain AI { get; private set; }

    public void Start()
    {
        AI = GetComponent<EnemyBrain>();
        EnemyUI = GetComponent<EnemyUI>();
        EnemyUI.Initialize();
        Health = GetComponent<Health>();
        EnemyUI.UpdateHealthBar(Health);
        Attack = GetComponent<Attack>();
    }



    // Stuff for debugging pathfinding
    protected const bool DEBUGPATHFINDINGMODE = true;
    [HideInInspector] public Color enemyColor;
    [HideInInspector] public float enemyLineY; // so that all the lines are on different y levels.



    public override float GetPathfindingCost()
    {
        return 4f; // Other enemies could wait for this one to move.
    }

    public void ProcessTurn()
    {
        AI.ActionPhase();
    }

    protected void StartMovementTowards(Vector2Int target)
    {
        MoveToPosition(target);
    }

    // Returns a list of points towards a target from where we're standing.
    public List<Point> FindPathTo(int x, int z, Pathfinding.DistanceType distanceType)
    {
        Point _from = new Point(xPos, zPos);
        Point _to = new Point(x, z);
        return Pathfinding.FindPath(BattleManager.instance.map.walkGrid, _from, _to, distanceType);
    }

    public override bool GetPlayerWalkable()
    {
        return false;
    }

    public override int TakeDamage(int amount)
    {
        int oldHealth = Health.CurrentHealth;
        Health.TakeDamage(amount);
        int damageDealt = oldHealth - Health.CurrentHealth;
        BattleManager.cardResolveStack.AddDamageDealt(damageDealt);
        EnemyUI.UpdateHealthBar(Health);
        if (Health.IsDead)
            Eliminate(); // KO'd
        else
        {
            EnemyUI.FadeHealthBar();
        }
        return damageDealt;
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
        // Trigger on death effects
        OnDeath();

        // Remove self from the battlegrid
        BattleGrid.instance.ClearTile(new Vector2Int(xPos, zPos));

        // Disengage
        BattleManager.player.RemoveEngagedEnemy(this);

        // Destroy self
        BattleManager.RecursivelyEliminateObject(transform);
    }

    public void OnDeath()
    {
        // Spawn some money
        Debug.Log("Spawning monies");
        BattleManager.instance.map.SpawnMoneyOnTile(new Vector2Int(xPos, zPos), UnityEngine.Random.Range(10, 22));
    }

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
