using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;
using Roguelike;

// This goes onto the player holder.
public class PlayerController : TileCreature
{
    private const int movementEnergyCost = 1;
    private bool isMoving = false;
    public Rigidbody rb;
    public BoxCollider boxCollider;

    // Movement Variables
    public Vector3 moveTarget;
    public float movementTime = 1f; // Time we want it to take one tile of movement.
    private float inverseMovementTime;
    //private bool controllingCamera = false;
    //private bool movementLocked = false; // If this is true, cannot move.


    // Card Management
    private Deck playerDeck;
    // The gameobject that exists on the canvas. Make the card templates children of this.
    public PlayerUIManager puim; // This class manages the player's ui graphically.
    
    private int cardRedrawAmount = 8; // Redraw up to this number of cards.
    private int maxTurnsUntilDraw = 2; // Draw a card every X turns.
    private int turnsUntilDraw = 2;

    // Resources
    private int currentEnergy;
    private int energyPerTurn = 3;
    private float currentSpirit;
    private int maxSpirit = 1000;
    private int spiritCostPerDiscard = 15; // How much spirit it costs to discard a card
    private int health;
    private int maxHealth = 100;

    // LOS
    public bool[,] LoSGrid; // A grid that shows which tiles the player has LOS to relative to themselves. Used for un-obscuring the camera.
    public bool[,] SimpleLoSGrid; // Uses simple middle-to-middle LoS. Used for non-corner cutting attacks.

    private List<TileEntity> engagedEnemies;

    public float CurrentSpirit
    {
        get { return currentSpirit; }
        set { currentSpirit = value; puim.SetSpiritPercentage(currentSpirit / maxSpirit); } // Automatically update the display whenever spirit changes
    }
    public int CurrentEnergy 
    {
        get { return currentEnergy; }
        set {currentEnergy = value; puim.SetCurrentEnergy(currentEnergy); }
    }
    public int Health
    {
        get { return health; }
        set { health = value; puim.SetCurrentHealth(health); }
    }

    void Awake()
    {
    }

    // Things to set once everything else has been initialized
    private void Start()
    {
        AssignVariables();
        ApplyStatusEffect(BattleManager.StatusEffectEnum.defense, 4);
        ApplyStatusEffect(BattleManager.StatusEffectEnum.defense, 5);
        ApplyStatusEffect(BattleManager.StatusEffectEnum.inspiration, 2);
        ApplyStatusEffect(BattleManager.StatusEffectEnum.momentum, 5);
    }

    // 0 means cannot be moved through
    public override float GetPathfindingCost()
    {
        return 1.0f;
    }
    public override bool GetPlayerWalkable()
    {
        return false;
    }

    private void AssignVariables()
    {
        // Movement / physics
        inverseMovementTime = 1f / movementTime;
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        puim = GetComponent<PlayerUIManager>();
        moveTarget = transform.position;

        // Cards
        AssignInitialDeck();

        CurrentEnergy = energyPerTurn;
        CurrentSpirit = maxSpirit;

        LoSGrid = BattleGrid.instance.RecalculateLOS(visionRange, out SimpleLoSGrid);

        Health = maxHealth;
        engagedEnemies = new List<TileEntity>();

        statusEffects = new Dictionary<BattleManager.StatusEffectEnum, StatusEffectDataHolder>();
    }

    // Builds the player's starting deck
    private void AssignInitialDeck()
    {
        if (Deck.instance == null)
            playerDeck = new Deck();

        if (puim == null)
            puim = GetComponent<PlayerUIManager>();

        // Placeholder cards.
        Card claws = new CardClaws();
        claws.owner = this;
        Card footwork = new CardFootwork();
        footwork.owner = this;
        Card cinders = new CardCinders();
        cinders.owner = this;
        Card dragonheart = new CardDragonHeart();
        dragonheart.owner = this;
        playerDeck.InsertCardAtEndOfDrawPile(claws);
        playerDeck.InsertCardAtEndOfDrawPile(claws);
        playerDeck.InsertCardAtEndOfDrawPile(claws);
        playerDeck.InsertCardAtEndOfDrawPile(claws);
        playerDeck.InsertCardAtEndOfDrawPile(claws);
        playerDeck.InsertCardAtEndOfDrawPile(claws);
        playerDeck.InsertCardAtEndOfDrawPile(claws);
        playerDeck.InsertCardAtEndOfDrawPile(footwork);
        playerDeck.InsertCardAtEndOfDrawPile(footwork);
        playerDeck.InsertCardAtEndOfDrawPile(cinders);
        playerDeck.InsertCardAtEndOfDrawPile(cinders);
        playerDeck.InsertCardAtEndOfDrawPile(cinders);
        playerDeck.InsertCardAtEndOfDrawPile(dragonheart);

        playerDeck.ShuffleDeck();
        DrawToHandLimit();
    }

    // Every X turns, we draw one card if we're below max.
    private void StartOfTurnDraw()
    {
        //Debug.Log("PlayerController::EndOfTurnDraw() -- TurnsUntilDraw: " + turnsUntilDraw + ", we have " + playerDeck.hand.Count + " cards, vs. a limit of " + cardRedrawAmount + " cards.");
        turnsUntilDraw--;
        if (turnsUntilDraw == 0)
        {
            if (playerDeck.hand.Count < cardRedrawAmount)
            {
                // Draw one card.
                Card drawnCard = playerDeck.DrawCard();
                if (drawnCard == null)
                {
                    Debug.Log("PlayerController::EndOfTurnDraw() -- CANNOT DRAW CARD!");
                    turnsUntilDraw = 1; // Cannot draw.
                    puim.SetTurnsUntilDrawText(1);
                    return;
                }
                // Now spawn in the card.
                puim.SpawnCardInHand(drawnCard);
                turnsUntilDraw = maxTurnsUntilDraw;
            }
            else
            {
                turnsUntilDraw = 1; // Stay at one turn
            }
        }
        puim.SetTurnsUntilDrawText(turnsUntilDraw);
    }

    private void DrawToHandLimit()
    {
        while (playerDeck.hand.Count < cardRedrawAmount)
        {
            Card drawnCard = playerDeck.DrawCard();
            if (drawnCard == null)
                break; // Cannot draw.

            // Now spawn in the card.
            puim.SpawnCardInHand(drawnCard);
        }
    }

    public enum AttemptToPlayCardFailReasons { success, notPlayerTurn, notEnoughEnergy};
    // Attempts to play that card index on given tile.
    public AttemptToPlayCardFailReasons AttemptToPlayCard(int index, Vector2Int target)
    {
        // Check if we can play card
        if (BattleManager.currentTurn != BattleManager.TurnPhase.player)
            return AttemptToPlayCardFailReasons.notPlayerTurn;

        Card cardToPlay = playerDeck.hand[index];

        // Do we have the energy for it?
        if (cardToPlay.energyCost > CurrentEnergy)
        {
            return AttemptToPlayCardFailReasons.notPlayerTurn;
        }

        // Play the card
        TriggerCardPlay(cardToPlay, target);
        // Discard it.
        playerDeck.DiscardCardAtIndex(index);
        return AttemptToPlayCardFailReasons.success;
    }

    // Plays the relevant card. Does not discard it afterwards.
    private void TriggerCardPlay(Card cardToPlay, Vector2Int target, bool resetTrackerWhenDone = true)
    {
        // Start card tracking
        BattleManager.instance.StartCardTracking(cardToPlay);

        // Pay the cost of the card
        PayCardCost(cardToPlay);

        // Activate its effects.
        cardToPlay.CardPlayEffect(target);

        if (resetTrackerWhenDone)
            ResolveCardStack();
    }

    // Called at the end of a stack of card effects.
    private void ResolveCardStack()
    {
        BattleManager.instance.StopCardTracking(); 
    }

    // Deducts the energy and spirit cost of the card.
    private void PayCardCost(Card cardData)
    {
        CurrentEnergy -= cardData.energyCost;
        LoseSpirit(cardData.spiritCost);
    }

    // Discards this card and fixes the indexes for the others.
    private void DiscardCardIndex(int index)
    {
        // Discard card data
        playerDeck.DiscardCardAtIndex(index);
        puim.DiscardCardAtIndex(index);
    }

    // Called by the puim when the player forcibly tries to discard a card
    public void ManuallyDiscardCardAtIndex(int index)
    {
        playerDeck.DiscardCardAtIndex(index);
        puim.DiscardCardAtIndex(index);
        LoseSpirit(spiritCostPerDiscard);
    }

    // Update is called once per frame
    void Update()
    {
        // Hitting P will toggle control camera
        if (Input.GetKeyDown(KeyCode.P))
            if (puim.IsStateControllingCamera())
                puim.MoveToState(PlayerUIManager.PlayerUIState.standardNoCardDrawer);
            else
                puim.MoveToState(PlayerUIManager.PlayerUIState.controllingCamera);

        HandleMovement();
    }

    private void HandleMovement()
    {


        if (isMoving) // If we are moving, keep going.
        {
            MoveTowardsTarget();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            // Hitting enter ends turn.
            EndOfTurn();
            return;
        }
        else
        {
            bool movementLocked = puim.IsStateMovementLocked();
            if (!movementLocked && BattleManager.currentTurn == BattleManager.TurnPhase.player)
            {
                HandleMovementInput(); // Get input and pick somewhere to move.
            }
            else if (puim.IsStateControllingCamera())
            {
                Debug.Log("HandleCameraMovement");
                puim.HandleCameraMovement();
            }

        }
    }

    private void MoveTowardsTarget()
    {
        // Lerp our position towards the target. If we've reached it, unset isMoving.
        transform.position = Vector3.MoveTowards(transform.position, moveTarget, inverseMovementTime * Time.deltaTime);
        //Debug.Log("Moved to " + transform.position);
        
        // Now we check if we've hit the target.
        if (Math.Abs(transform.position.x - moveTarget.x) < Mathf.Epsilon && Math.Abs(transform.position.z - moveTarget.z) < Mathf.Epsilon)
        {
            // We've hit the target.
            isMoving = false;
        }
    }

    private void HandleMovementInput()
    {
        // First, get the movement.

        int xDir = (int)(Input.GetAxisRaw("Horizontal"));
        int zDir = (int)(Input.GetAxisRaw("Vertical"));

        // Check if we're moving anywhere
        if (xDir != 0 || zDir != 0)
        {
            // Check if we're holding L
            if (Input.GetKey(KeyCode.L))
            {
                // Only able to move on diagonals while holding L.
                if (xDir == 0 || zDir == 0)
                    return;
            }
            Vector2Int movementVector = new Vector2Int(xDir + xPos, zDir + zPos);
            if (CheckCanMoveInDirection(xDir, zDir))
            {
                MoveTo(movementVector, true);
                EndOfTurn();
            }
                
        }

        // Otherwise, do nothing.
    }

    // Checks to see if we can move in that direction.
    // Uses the battlegrid.
    private bool CheckCanMoveInDirection(int xDir, int zDir)
    {
        // Do we have enough energy?
        if (currentEnergy < movementEnergyCost)
        {
            puim.ShowAlert("Not enough energy. Press enter to end turn.");
            return false;
        }

        try
        {
            if (xDir * zDir == 0)
            {
                // We're not moving in a diagonal. Just check if the target is open.
                Tile target = BattleManager.instance.GetTileAtLocation(xPos + xDir, zPos + zDir);
                //Debug.Log("Checking " + (xPos + xDir) + "," + (zPos + zDir));
                return target.GetPlayerWalkability();
            }
            else
            {
                // We're moving in a diagonal. Need to check both cardinal directions and the diagonal.
                Tile xtarget = BattleManager.instance.GetTileAtLocation(xPos + xDir, zPos);
                Tile ztarget = BattleManager.instance.GetTileAtLocation(xPos, zPos + zDir);
                Tile xztarget = BattleManager.instance.GetTileAtLocation(xPos + xDir, zPos + zDir);

                return xtarget.GetPlayerWalkability() && ztarget.GetPlayerWalkability() && xztarget.GetPlayerWalkability();
            }
        }
        catch (IndexOutOfRangeException)
        {
            // Well, if we're trying to move off the map, then we can't go this way.
            return false;
        }
    }

    // Checks to see if we can move in that direction.
    // Uses raycasting and collision detection
    /*
    private bool CheckCanMove(int xDir, int zDir)
    {
        Debug.Log("Checking collision");
        // Disable own collider for a moment
        boxCollider.enabled = false;
        if (xDir * zDir == 0)
        {
            // This means that one of these two is 0. So we're not moving in a diagonal.
            // Make a raycast to see if we hit anything in between where we are and where we want to go.
            Debug.Log("Checking collision from " + transform.position + " to " + moveTarget);
            bool tarHit = Physics.Linecast(transform.position, moveTarget, out RaycastHit hit, movementBlockingLayer);
            ResolveMovementCheck(hit);
            Debug.Log("Result = " + tarHit);
            if (!tarHit)
                return true; // We didn't hit anything, so we're good to go.
            return false;
        }
        else
        {
            // Both xDir and zDir are non-zero. This means we're moving diagonal.
            // Need to check both directions and the diagonal. We don't want to be able to cut corners.
            Vector3 xTar = new Vector3(transform.position.x + xDir, transform.position.y, transform.position.z);
            bool xDirHit = Physics.Linecast(transform.position, xTar, out RaycastHit hit, movementBlockingLayer);
            if (xDirHit)
            {
                ResolveMovementCheck(hit);
                return false;
            }

            // Now check zDir
            Vector3 zTar = new Vector3(transform.position.x, transform.position.y, transform.position.z + zDir);
            bool zDirHit = Physics.Linecast(transform.position, xTar, out hit, movementBlockingLayer);
            if (zDirHit)
            {
                ResolveMovementCheck(hit);
                return false;
            }

            // Finally, check the diagonal itself.
            bool diagHit = Physics.Linecast(transform.position, moveTarget, out hit, movementBlockingLayer);
            ResolveMovementCheck(hit);
            if (diagHit)
                return false;
            return true;
        }
    }
    */

    // Updates the location of the player in the battlegrid. If movePhyisically, also
    // moves the player holder in Unity space.
    public void MoveTo(Vector2Int newMoveTarget, bool movePhysically, float yLevel = -1.123f)
    {
        if (yLevel == -1.123f)
        {
            yLevel = transform.position.y;
        }

        if (movePhysically)
        {
            // This may be in the middle of a movement. Force finish if so.
            if (isMoving)
            {
                transform.position = moveTarget;
            }
            moveTarget = BattleManager.ConvertVector(newMoveTarget, yLevel) ;

            // Set up our variables to start movement
            isMoving = true;
            MoveTowardsTarget();
        }

        // Update position
        BattleGrid.instance.MoveObjectTo(newMoveTarget, this);

        // Recalculate LOS
        LoSGrid = BattleGrid.instance.RecalculateLOS(visionRange, out SimpleLoSGrid);
    }

    // Applies incoming damage
    public override void TakeDamage(float damage)
    {
        // Do we have defense?
        if (statusEffects.TryGetValue(BattleManager.StatusEffectEnum.defense, out StatusEffectDataHolder val))
        {
            int oldDamage = (int)damage;
            damage -= val.EffectValue;
            ApplyStatusEffect(BattleManager.StatusEffectEnum.defense , - 1 * oldDamage); // Deal damage to defense
        }

        if (damage > 0)
        {
            Health -= (int)damage;
        }
    }

    // Handles stuff that happens at the end of the player turn.
    private void EndOfTurn()
    {

        // Spirit decay
        LoseSpirit(1);

        puim.EndOfTurn();
        
        BattleManager.instance.EndOfTurn();

        StartOfTurn();
    }

    private void StartOfTurn()
    {
        // Defense decay
        StartOfTurnStatusDecay();

        // Refill energy
        CurrentEnergy = energyPerTurn;
        puim.SetCurrentEnergy(CurrentEnergy);

        // Draw
        StartOfTurnDraw();
    }

    private void StartOfTurnStatusDecay()
    {
        // Do we have defense?
        if (statusEffects.TryGetValue(BattleManager.StatusEffectEnum.defense, out StatusEffectDataHolder val))
        { // Lose one defense
            ApplyStatusEffect(BattleManager.StatusEffectEnum.defense, -1); 
        }
    }

    // Causes a loss of this much spirit. Applies reductions from low spirit.
    // Lose .5% less per 1% lost.
    public void LoseSpirit(float amount)
    {
        float lossMult = 1 - (0.5f * (1 - (CurrentSpirit / maxSpirit)));
        //Debug.Log("Currently have " + CurrentSpirit.ToString("0.00") + "/" + maxSpirit + " spirit, which makes the loss mult " + lossMult);
        CurrentSpirit -= amount * lossMult;
        if (CurrentSpirit < 0)
            CurrentSpirit = 0;
    }

    // Returns true on a success
    public bool RemoveEngagedEnemy(TileEntity tar)
    {
        return engagedEnemies.Remove(tar);
    }

    public void AddEngagedEnemy(TileEntity tar)
    {
        engagedEnemies.Add(tar);
    }

    public override int GetStatusEffectValue(BattleManager.StatusEffectEnum status)
    {
        if (statusEffects.TryGetValue(status, out StatusEffectDataHolder x))
        {
            return x.EffectValue;
        }

        return 0;
    }

    // Adds amount to the status effect.
    public override void ApplyStatusEffect(BattleManager.StatusEffectEnum status, int amount)
    {
        if (statusEffects.ContainsKey(status))
        {
            StatusEffectDataHolder x = statusEffects[status];
            // we have the status effect. Add to it.
            x.EffectValue += amount;

            // If the value is <= 0, we need to remove it.
            if (x.EffectValue <= 0)
            {
                puim.RemoveStatusEffect(x);
                statusEffects.Remove(status);
            }
        }
        else
        {
            // We don't have the status effect. Add it.
            statusEffects.Add(status, puim.AddNewStatusEffect(status, amount));
        }
    }

}
