using NesScripts.Controls.PathFind;
using System.Collections.Generic;
using UnityEngine;

public class BossBrain : EnemyBrain
{
    public bool Activated = false;
    public BossRoom BossRoom { get; set; }
    public float ChanceOfSpawningMinion = .25f;

    private EnemyBody Body;

    public void Awake()
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
            //Else move closer to enemy
            else
            {
                BattleGrid.instance.CurrentFloor.ClearBossTiles();

                List<Point> pathToCombatTarget = Body.FindPathTo(player.xPos, player.zPos, Pathfinding.DistanceType.NoCornerCutting);
                pathToCombatTarget = TakeStepInPath(pathToCombatTarget, false);

                BattleGrid.instance.CurrentFloor.PlaceBoss(Body);

            }
        }
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
        return;
    }

    public override void StartOfTurn()
    {
        if (!Activated)
        {
            Activated = BossRoom.ActivateBoss();
        }
        if (Activated)
        {
            if (Random.RandBool(ChanceOfSpawningMinion))
            {
                var spawnLoc = MinionSpawnLocations().PickRandom();
                var minion = EnemySpawner.SpawnMinion(spawnLoc);
                var body = minion.GetComponent<EnemyBody>();
                BattleGrid.instance.CurrentFloor.PlaceObjectOn(spawnLoc.x, spawnLoc.y, body);
                BattleGrid.instance.CurrentFloor.map[spawnLoc.x, spawnLoc.y].tileEntityType = Roguelike.Tile.TileEntityType.enemy;
                BattleGrid.instance.CurrentFloor.enemies.Add(body);
            }
        }
    }

    private List<Vector2Int> MinionSpawnLocations()
    {
        var res = new List<Vector2Int>();
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                var x = Body.xPos + i;
                var y = Body.zPos + j;
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
    }

    public override bool IsDoneWithAction()
    {
        return Body.Attack.IsAttackDone && (Body.Animation.IsIdle() || Body.Animation.IsMoving());
    }
}

