using NesScripts.Controls.PathFind;
using NesScripts.Controls.PathFind;
using Roguelike;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicMeleeEnemy : GenericEnemy
{
    private bool inStrikingRange = false;

    public new void Start()
    {
        base.Start();
        if (true)
        {
            enemyColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            enemyLineY = Random.Range(0.05f, 0.8f);
        }

        moveTarget = transform.position;
        maxHealth = 40;
        Health = maxHealth;
        visionRange = 5;
    }

    private void Update()
    {

    }

    public override void ProcessTurn()
    {
        StandardEnemyTurn();
    }

    protected override void TakeIntent()
    {
        throw new System.NotImplementedException();
    }

    protected override void DetermineIntent()
    {
        throw new System.NotImplementedException();
    }

    // We've seen the player. What should we do?
    // This enemy plans to take a step forward and attack.
    // If it ends its turn within 2 tiles of you, it will attack next turn.
    protected override void DecideCombatTurn()
    {
        // Find path to player. We can assume the player will move a lot, so we should recalc often.
        List<Point> pathToCombatTarget = FindPathTo(engagedTarget.xPos, engagedTarget.zPos, Pathfinding.DistanceType.NoCornerCutting);
        DebugDrawPath(pathToCombatTarget);

        if (pathToCombatTarget.Count == 0)
        {
            // Uh... We can see the target, but not reach them?
            Debug.LogError("BasicMeleeEnemy--DecideCombatTurn()::I can see my enemy but I can't reach them. You didn't tell me you added windows to the game! You think we have the budget for that?");
            return;
        }

        if (pathToCombatTarget.Count > 1) // If we're not adjacent
        {
            pathToCombatTarget = TakeStepInPath(pathToCombatTarget, false);
        }

        if (inStrikingRange) // Strike the tile nearby
        {
            LaunchAttack(BattleManager.ConvertPoint(pathToCombatTarget[0]));
        }

        inStrikingRange = (pathToCombatTarget.Count <= 2);
    }

    protected override void LaunchAttack(Vector2Int target)
    {
        BattleGrid.instance.StrikeTile(target, damage);
    }

    protected override void InitCombat()
    {
        // If we're within 2 tiles of player, readyToStrike
        inStrikingRange = (Vector3.Distance(transform.position, engagedTarget.transform.position) <= 2);
    }

    public override void ApplyStatusEffect(BattleManager.StatusEffectEnum status, int amount)
    {
        Debug.LogError("Generic Enemy -- I was asked to apply a status effect, but that's not implemented.");
    }

    public override void OnDeath()
    {
        // Spawn some money
        Debug.Log("Spawning monies");
        BattleManager.instance.map.SpawnMoneyOnTile(new Vector2Int(xPos, zPos), Random.Range(10, 22));
    }
}
