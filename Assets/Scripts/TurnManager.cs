using Roguelike;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public PlayerController Player;
    public enum WhoseTurn { player, enemy };  // Whose turn it currently is
    public enum TurnPhase { start, action, moving, end };
    public WhoseTurn CurrentTurn { get; set; } = WhoseTurn.player;
    public TurnPhase CurrentPhase { get; set; } = TurnPhase.start;

    private bool waiting = false;   //Not happy with this solution but its quick

    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void Update()
    {
        //Update fog of war every frame. Probably should change this to be more efficient
        BattleGrid.instance.FogOfWar.ForceUpdate();

        //End game if they press escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
        }

        if (!BattleGrid.instance.LoadingNewFloor)
        {
            if (CurrentTurn == WhoseTurn.player)
            {
                HandlePlayerTurn();
            }
            if (CurrentTurn == WhoseTurn.enemy && !BattleGrid.instance.LoadingNewFloor)
            {
                HandleEnemyTurn();
            }
        }
    }

    /// <summary>
    /// Returns whether or not the player can currently play a card
    /// </summary>
    /// <returns></returns>
    public bool CanPlayCard()
    {
        return CurrentTurn == WhoseTurn.player && CurrentPhase == TurnPhase.action;
    }

    private void HandlePlayerTurn()
    {
        if (!AreTrapsDone())
        {
            return;
        }
        if (CurrentPhase == TurnPhase.start)
        {
            //Start of player's turn. Draw cards, handle status effects, etc
            Player.StartOfTurn();
            CurrentPhase = TurnPhase.action;
        }

        if (CurrentPhase == TurnPhase.action)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                CurrentPhase = TurnPhase.end;
                return;
            }
            else
            {
                if (Player.HandleMovement())
                {
                    CurrentPhase = TurnPhase.end;
                }
            }
        }

        //Wait for player to finish moving before ending turn
        if (CurrentPhase == TurnPhase.end && !Player.IsMoving)
        {
            Player.EndOfTurn();
            CurrentPhase = TurnPhase.start;
            CurrentTurn = WhoseTurn.enemy;
        }
    }

    private void HandleEnemyTurn()
    {
        if (!AreTrapsDone())
        {
            return;
        }
        var enemies = new List<EnemyBody>(BattleGrid.instance.CurrentFloor.enemies);
        if (CurrentPhase == TurnPhase.start)
        {
            if (!waiting)
            {
                foreach (var enemy in enemies)
                {
                    enemy.AI.StartOfTurn();
                }
                waiting = true;
            }

            if (enemies.All(enemy => enemy.AI.IsDoneWithStart()))
            {
                CurrentPhase = TurnPhase.action;
                waiting = false;
            }
        }
        if (CurrentPhase == TurnPhase.action)
        {
            if (!waiting)
            {
                foreach (var enemy in enemies)
                {
                    enemy.AI.ActionPhase();
                }
                waiting = true;

            }

            foreach (var enemy in enemies)
            {
                var done = enemy.AI.IsDoneWithAction();
            }
            if (enemies.All(enemy => enemy.AI.IsDoneWithAction()))
            {
                CurrentPhase = TurnPhase.end;
                waiting = false;
            }
        }
        if (CurrentPhase == TurnPhase.end)
        {
            if (!waiting)
            {
                foreach (var enemy in enemies)
                {
                    enemy.AI.EndOfTurn();
                }
                waiting = true;
            }

            if (enemies.All(enemy => enemy.AI.IsDoneWithEndOfTurn()))
            {
                waiting = false;
                //Start player's turn right away so there isn't a delay when we are walking
                CurrentTurn = WhoseTurn.player;
                CurrentPhase = TurnPhase.start;
                if (!BattleGrid.instance.LoadingNewFloor)
                {
                    HandlePlayerTurn();
                }
            }
        }

    }

    private bool IsPlayerOnStairs()
    {
        var playerLocation = BattleManager.ConvertVector(Player.transform.position);
        var tile = BattleManager.instance.GetTileAtLocation(playerLocation);
        return tile.tileTerrainType == Tile.TileTerrainType.stairsDown;
    }

    public bool AreTrapsDone()
    {
        foreach (var trap in BattleGrid.instance.CurrentFloor.traps)
        {
            if (trap.IsActive)
            {
                return false;
            }
        }
        return true;
    }
}

