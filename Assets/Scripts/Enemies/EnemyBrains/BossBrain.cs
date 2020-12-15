using NesScripts.Controls.PathFind;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBrain : EnemyBrain
{
    public bool Activated = false;
    public BossRoom BossRoom { get; set; }
    public float ChanceOfSpawningMinion = .25f;

    private BossBody Body;
    private bool IsSpawningMinion = false;

    public bool HasBeenInvincible = false;
    public int InvincibleTurns = 15;
    public int NumberMinionsInWave = 10;
    public int HealthRegainedWhileInvulnerable = 2;
    private int InvincibleTurnsLeft;

    public void Awake()
    {
        Body = GetComponent<BossBody>();
    }

    public override void ActionPhase()
    {
        if (Activated && !Body.Invincible)
        {
            var player = BattleManager.player;

            if (Body.LaserAttack.IsTargetInRange(BattleManager.ConvertVector(player.transform.position)) && Random.RandBool(.4f))
            {
                LaserAttack();
                return;
            }
            if (Random.RandBool(.33f))
            {
                if (SpawnMinionAttack())
                {
                    return;
                }
            }
            if (Body.Melee.IsTargetInRange(BattleManager.ConvertVector(player.transform.position)))
            {
                MeleeAttack();
            }
            else
            {
                MoveCloserToPlayer();
            }
        }
    }

    private bool SpawnMinionAttack()
    {
        var playerLocation = BattleManager.ConvertVector(BattleManager.player.transform.position);
        var spawnLocations = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                var tile = BattleGrid.instance.CurrentFloor.map[playerLocation.x + i, playerLocation.y + j];
                if (tile.tileEntityType == Roguelike.Tile.TileEntityType.empty)
                {
                    spawnLocations.Add(new Vector2Int(playerLocation.x + i, playerLocation.y + j));
                }
            }
        }
        if (spawnLocations.Count < 2)
        {
            return false;
        }
        SpawnWaveOfMinions(2, spawnLocations, true);
        return true;
    }

    private void LaserAttack()
    {
        Body.Animation.LaserCast();
        Body.LaserAttack.ActivateAttack(BattleManager.ConvertVector(BattleManager.player.transform.position));
    }

    private void MeleeAttack()
    {
        var player = BattleManager.player;

        if (Body.Melee.IsTargetInRange(BattleManager.ConvertVector(player.transform.position)))
        {
            var animation = Body.Animation as BossAnimation;
            if (animation != null)
            {
                animation.Melee();
            }
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
    }

    private void MoveCloserToPlayer()
    {
        var player = BattleManager.player;

        BattleGrid.instance.CurrentFloor.ClearBossTiles();

        List<Point> pathToCombatTarget = Body.FindPathTo(player.xPos, player.zPos, Pathfinding.DistanceType.NoCornerCutting);
        pathToCombatTarget = TakeStepInPath(pathToCombatTarget, false);

        BattleGrid.instance.CurrentFloor.PlaceBoss(Body);
    }

    // Takes the next step in a list of points and returns the same list, minus the point at index 0 if successful
    protected List<Point> TakeStepInPath(List<Point> path, bool recalcIfFail)
    {
        if (path != null && path.Count > 0)
        {
            // Take the next step.
            Point nextStep = path[0];
            Vector2Int nextStepVector = BattleManager.ConvertPoint(nextStep);


            // Is this step valid?
            if (IsMoveValid(nextStepVector))
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
                MoveTo(nextStepVector);
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
        }
        return path;
    }

    private void MoveTo(Vector2Int location)
    {
        Body.xPos = location.x;
        Body.zPos = location.y;
        BattleGrid.instance.CurrentFloor.PlaceBoss(Body);
        Body.MoveToPosition(location);
    }

    private bool IsMoveValid(Vector2Int location)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                var x = location.x + i;
                var y = location.y + j;
                var tile = BattleGrid.instance.CurrentFloor.map[x, y];

                if (tile.tileEntityType == Roguelike.Tile.TileEntityType.enemy ||
                    tile.tileEntityType == Roguelike.Tile.TileEntityType.player ||
                    tile.tileEntityType == Roguelike.Tile.TileEntityType.wall)
                {
                    return false;
                }
            }
        }
        return true;
    }


    public override void EndOfTurn()
    {
        if (Activated)
        {
            if (InvincibleTurnsLeft > 0)
            {
                if (InvincibleTurnsLeft == 1)
                {
                    Body.Animation.BecomeVincible();
                    Body.Invincible = false;
                }
                InvincibleTurnsLeft--;
                Body.Health.Heal(HealthRegainedWhileInvulnerable);
            }
            else
            {
                if (Random.RandBool(ChanceOfSpawningMinion))
                {
                    SpawnMinion();
                }
            }

        }
    }

    public override void StartOfTurn()
    {
        if (!Activated)
        {
            Activated = BossRoom.ActivateBoss();
        }
        if (InvincibleTurnsLeft == InvincibleTurns)
        {
            Body.Animation.BecomeInvincible();
            Body.Invincible = true;
            SpawnWaveOfMinions(NumberMinionsInWave);
        }
        else if (!Body.Invincible && Body.Health.CurrentHealth <= Body.Health.MaxHealth / 2 && Random.RandBool(.2f))
        {
            Body.Animation.BecomeInvincible();
            Body.Invincible = true;
            SpawnWaveOfMinions(NumberMinionsInWave);
            InvincibleTurnsLeft = InvincibleTurns;
        }
    }

    private void SpawnWaveOfMinions(int numMinions)
    {
        var spawnLocations = BossRoom.GetPossibleSpawnLocations();
        spawnLocations.RemoveAll(spawnLoc =>
        {
            var tile = BattleGrid.instance.CurrentFloor.map[spawnLoc.x, spawnLoc.y];
            return tile.tileEntityType != Roguelike.Tile.TileEntityType.empty;
        });
        SpawnWaveOfMinions(numMinions, spawnLocations);
    }

    private void SpawnWaveOfMinions(int numMinions, List<Vector2Int> spawnLocations, bool glowing = false)
    {
        for (int i = 0; i < Math.Min(numMinions, spawnLocations.Count); i++)
        {
            var spawnLoc = Random.PickRandom(spawnLocations);
            spawnLocations.Remove(spawnLoc);
            SpawnMinion(spawnLoc, glowing);
        }
    }

    private void SpawnMinion()
    {
        var spawnLoc = Random.PickRandom(MinionSpawnLocations());
        SpawnMinion(spawnLoc);
    }

    private void SpawnMinion(Vector2Int spawnLoc, bool glowingAnimation = true)
    {
        IsSpawningMinion = true;
        StartCoroutine(SpawnMinionCoroutine(spawnLoc, glowingAnimation));
    }

    IEnumerator SpawnMinionCoroutine(Vector2Int spawnLoc, bool glowingAnimation)
    {
        if (glowingAnimation)
        {
            Body.Animation.Glowing();
        }
        Body.Animation.MinionSpawnEffect(spawnLoc);
        yield return new WaitForSeconds(.25f);
        var minion = EnemySpawner.SpawnMinion(spawnLoc);
        var body = minion.GetComponent<EnemyBody>();
        BattleGrid.instance.CurrentFloor.PlaceObjectOn(spawnLoc.x, spawnLoc.y, body);
        BattleGrid.instance.CurrentFloor.map[spawnLoc.x, spawnLoc.y].tileEntityType = Roguelike.Tile.TileEntityType.enemy;
        BattleGrid.instance.CurrentFloor.enemies.Add(body);
        yield return new WaitForSeconds(.75f);
        IsSpawningMinion = false;
    }

    private List<Vector2Int> MinionSpawnLocations()
    {
        var res = new List<Vector2Int>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                var x = BattleManager.player.xPos + i;
                var y = BattleManager.player.zPos + j;
                if (BattleGrid.instance.CurrentFloor.map[x, y].tileEntityType == Roguelike.Tile.TileEntityType.empty)
                {
                    res.Add(new Vector2Int(x, y));
                }
            }
        }
        return res;
    }

    public override void SummonForHelp(List<Point> pathToFollow)
    {
        //The boss doesn't respond to calls for help
        return;
    }

    public override void OnDeath()
    {
        BattleGrid.instance.CurrentFloor.ClearBossTiles();
        BattleGrid.instance.CurrentFloor.enemies.Remove(Body);
        var enemies = BattleGrid.instance.CurrentFloor.enemies;
        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            if (enemies[i].AI is MinionBrain)
            {
                enemies[i].Eliminate();
            }
        }
        ((BossLevel)BattleGrid.instance.CurrentFloor.Level).BossRoom.UnlockRoom();
    }

    public override bool IsDoneWithAction()
    {
        return Body.Melee.IsAttackDone && Body.LaserAttack.IsAttackDone && !IsSpawningMinion && (Body.Animation.IsIdle() || Body.Animation.IsMoving() || Body.Invincible);
    }

    public override bool IsDoneWithEndOfTurn()
    {
        return !IsSpawningMinion;
    }

    public void BecomeInvicible()
    {
        HasBeenInvincible = true;
        InvincibleTurnsLeft = InvincibleTurns;
    }
}

