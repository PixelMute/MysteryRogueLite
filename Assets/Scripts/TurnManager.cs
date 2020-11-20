using Roguelike;
using UnityEngine;

class TurnManager : MonoBehaviour
{
    public static TurnManager instance;

    public PlayerController Player;
    public enum WhoseTurn { player, enemy };  // Whose turn it currently is
    private enum TurnPhase { start, action, moving, end };
    public WhoseTurn CurrentTurn { get; private set; } = WhoseTurn.player;
    private TurnPhase CurrentPhase = TurnPhase.start;

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
            if (CurrentTurn == WhoseTurn.enemy)
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

            //If player ends their turn on the stairs, go down to next floor
            if (IsPlayerOnStairs())
            {
                BattleGrid.instance.GoDownFloor();
                CurrentPhase = TurnPhase.start;
            }

            CurrentPhase = TurnPhase.start;
            CurrentTurn = WhoseTurn.enemy;
        }
    }

    private void HandleEnemyTurn()
    {
        var enemies = BattleGrid.instance.CurrentFloor.enemies;
        foreach (var enemy in enemies)
        {
            enemy.ProcessTurn();
        }

        //Start player's turn right away so there isn't a delay
        CurrentTurn = WhoseTurn.player;
        HandlePlayerTurn();
    }

    private bool IsPlayerOnStairs()
    {
        var playerLocation = BattleManager.ConvertVector(Player.transform.position);
        var tile = BattleManager.instance.GetTileAtLocation(playerLocation);
        return tile.tileTerrainType == Tile.TileTerrainType.stairsDown;
    }

}

