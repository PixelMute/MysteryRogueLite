using Roguelike;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR;

// This goes onto the player holder.
public class PlayerController : TileCreature
{
    private const int movementEnergyCost = 1;
    public bool isMoving = false;
    public Rigidbody rb;
    public BoxCollider boxCollider;

    // Movement Variables
    public Vector3 moveTarget;
    public float movementTime = 1f; // Time we want it to take one tile of movement.
    private float inverseMovementTime;
    //private bool controllingCamera = false;
    //private bool movementLocked = false; // If this is true, cannot move.


    // Card Management
    public static Deck playerDeck;
    // The gameobject that exists on the canvas. Make the card templates children of this.
    public PlayerUIManager puim; // This class manages the player's ui graphically.

    private int cardRedrawAmount = 8; // Redraw up to this number of cards.
    private int maxHandSize = 12;
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

    // Status effects

    private List<TileEntity> engagedEnemies;

    public float CurrentSpirit
    {
        get { return currentSpirit; }
        set { currentSpirit = value; puim.SetSpiritPercentage(currentSpirit / maxSpirit); } // Automatically update the display whenever spirit changes
    }
    public int CurrentEnergy
    {
        get { return currentEnergy; }
        set { currentEnergy = value; puim.SetCurrentEnergy(currentEnergy); }
    }
    public int Health
    {
        get { return health; }
        set { health = value; puim.SetCurrentHealth(health, maxHealth); }
    }

    void Awake()
    {
    }

    // Things to set once everything else has been initialized
    private void Start()
    {
        AssignVariables();
        //ApplyStatusEffect(BattleManager.StatusEffectEnum.defence, 4);
        //ApplyStatusEffect(BattleManager.StatusEffectEnum.defence, 5);
        //ApplyStatusEffect(BattleManager.StatusEffectEnum.insight, 2);
        //ApplyStatusEffect(BattleManager.StatusEffectEnum.momentum, 5);
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

        UpdateLOS();

        Health = maxHealth;
        engagedEnemies = new List<TileEntity>();

        statusEffects = new Dictionary<BattleManager.StatusEffectEnum, StatusEffectDataHolder>();
    }

    public void UpdateLOS()
    {
        LoSGrid = BattleGrid.instance.RecalculateLOS(visionRange, transform.position, out SimpleLoSGrid);
    }

    public void UpdateLOS(Vector2Int target)
    {
        LoSGrid = BattleGrid.instance.RecalculateLOS(visionRange, BattleManager.ConvertVector(target, transform.position.y), out SimpleLoSGrid);
    }

    // Builds the player's starting deck
    private void AssignInitialDeck()
    {
        playerDeck = new Deck();
        Deck.instance = playerDeck;

        if (puim == null)
            puim = GetComponent<PlayerUIManager>();

        // Placeholder cards.
        Card claws = CardFactory.GetCard("claws");
        claws.Owner = this;
        Card cinders = CardFactory.GetCard("cinders");
        cinders.Owner = this;
        Card footwork = CardFactory.GetCard("Footwork");
        footwork.Owner = this;
        Card dragonheart = CardFactory.GetCard("dragonheart");
        dragonheart.Owner = this;
        Card needles = CardFactory.GetCard("needles");
        needles.Owner = this;
        Card shadow = CardFactory.GetCard("shadowphase");
        shadow.Owner = this;
        Card thunderstep = CardFactory.GetCard("thunderstep");
        thunderstep.Owner = this;
        Card backflip = CardFactory.GetCard("backflip");
        backflip.Owner = this;
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

        // Non-default deck
        /*
        playerDeck.InsertCardAtEndOfDrawPile(needles);
        playerDeck.InsertCardAtEndOfDrawPile(shadow);
        playerDeck.InsertCardAtEndOfDrawPile(thunderstep);
        playerDeck.InsertCardAtEndOfDrawPile(thunderstep);
        playerDeck.InsertCardAtEndOfDrawPile(backflip);
        playerDeck.InsertCardAtEndOfDrawPile(backflip);*/

        playerDeck.ShuffleDeck();
        DrawToHandLimit();
    }

    /// <summary>
    /// Adds the selected card to the discard pile.
    /// </summary>
    /// <param name="cardData">Card to add</param>
    internal void GainCard(Card cardData)
    {
        cardData.Owner = this;
        playerDeck.discardPile.Add(cardData);
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
                if (!DrawCard())
                {
                    // Failed
                    turnsUntilDraw = 1; // Cannot draw.
                    puim.SetTurnsUntilDrawText(1);
                    return;
                }
                turnsUntilDraw = maxTurnsUntilDraw;
            }
            else
            {
                turnsUntilDraw = 1; // Stay at one turn
            }
        }
        puim.SetTurnsUntilDrawText(turnsUntilDraw);
    }

    /// <summary>
    /// WHAT DOES THE FUNCTION DRAWCARD() DO?
    /// IT ALLOWS ME TO DRAW ~~two~~ ONE CARD FROM MY DECK AND ADD IT TO MY HAND
    /// Returns true if successful
    /// </summary>
    public bool DrawCard()
    {
        if (playerDeck.hand.Count >= maxHandSize)
        {
            return false; // Too many cards. Cannot draw more.
        }

        Card drawnCard = playerDeck.DrawCard();
        if (drawnCard == null)
        {
            Debug.Log("PlayerController::DrawCard() -- CANNOT DRAW CARD!");
            return false;
        }
        // Now spawn in the card.
        puim.SpawnCardInHand(drawnCard);
        return true;
    }

    /// <summary>
    /// Adds energy to the player
    /// </summary>
    /// <param name="amount"></param>
    public void AddEnergy(int amount)
    {
        // Later, do stuff here.
        currentEnergy += amount;
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

    public enum AttemptToPlayCardFailReasons { success, notPlayerTurn, notEnoughEnergy };
    // Attempts to play that card index on given tile.
    public AttemptToPlayCardFailReasons AttemptToPlayCard(int index, Vector2Int target)
    {
        // Check if we can play card
        if (!TurnManager.instance.CanPlayCard())
            return AttemptToPlayCardFailReasons.notPlayerTurn;

        Card cardToPlay = playerDeck.hand[index];

        // Do we have the energy for it?
        if (cardToPlay.CardInfo.EnergyCost > CurrentEnergy)
        {
            return AttemptToPlayCardFailReasons.notEnoughEnergy;
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
        cardToPlay.Activate(BattleManager.ConvertVector(transform.position), target);

        if (resetTrackerWhenDone)
            ResolveCardPlayed();
    }

    // Called at the end of a stack of card effects.
    private void ResolveCardPlayed()
    {
        // If we used momentium, clear all our momentium.
        if (BattleManager.cardResolveStack.QueryUsedMomentum())
        {
            if (statusEffects.TryGetValue(BattleManager.StatusEffectEnum.momentum, out var val))
            {
                ApplyStatusEffect(BattleManager.StatusEffectEnum.momentum, -1 * val.EffectValue); // Removes momentium
            }
        }

        BattleManager.instance.StopCardTracking();
    }

    // Deducts the energy and spirit cost of the card.
    private void PayCardCost(Card cardData)
    {
        CurrentEnergy -= cardData.CardInfo.EnergyCost;
        LoseSpirit(cardData.CardInfo.SpiritCost);
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
    }

    public void HandleMovement()
    {
        bool movementLocked = puim.IsStateMovementLocked();
        if (!movementLocked)
        {
            HandleMovementInput(); // Get input and pick somewhere to move.
        }
        else if (puim.IsStateControllingCamera())
        {
            Debug.Log("HandleCameraMovement");
            puim.HandleCameraMovement();
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
            var speedOfMovement = TimeToMove;
            if (xDir != 0 && zDir != 0)
            {
                //Adjust speed for the diagonals so we aren't moving faster then
                speedOfMovement = (float)Math.Sqrt(2) * TimeToMove;
            }
            // Check if we're holding L
            if (Input.GetKey(KeyCode.L))
            {
                // Only able to move on diagonals while holding L.
                if (xDir == 0 || zDir == 0)
                    return;
            }
            Vector2Int movementVector = new Vector2Int(xDir + xPos, zDir + zPos);

            var tile = BattleManager.instance.GetTileAtLocation(movementVector.x, movementVector.y);
            if (tile.tileEntityType == Tile.TileEntityType.stairsDown)
            {
                BattleGrid.instance.GoDownFloor();
                return;
            }

            if (CheckCanMoveInDirection(xDir, zDir))
            {
                MoveToPosition(movementVector, speedOfMovement);
                // Recalculate LOS
                UpdateLOS(movementVector);
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

    // Applies incoming damage
    public override void TakeDamage(int damage)
    {
        if (damage >= 0) // Damage
        {
            // Do we have defense?
            if (statusEffects.TryGetValue(BattleManager.StatusEffectEnum.defence, out StatusEffectDataHolder val))
            {
                int oldDamage = (int)damage;
                damage -= val.EffectValue;
                ApplyStatusEffect(BattleManager.StatusEffectEnum.defence, -1 * oldDamage); // Deal damage to defense
            }

            if (damage > 0)
            {
                Health -= damage;
            }
        }
        else // healing
        {
            Health -= damage;
            if (Health > maxHealth)
                Health = maxHealth;
        }

        if (Health <= 0)
        {
            GameOverScreen.PlayerDeath();
        }
    }

    // Handles stuff that happens at the end of the player turn.
    public void EndOfTurn()
    {
        // Spirit decay
        LoseSpirit(1);

        puim.EndOfTurn();
    }

    public void StartOfTurn()
    {
        // Status decay
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
        if (statusEffects.ContainsKey(BattleManager.StatusEffectEnum.defence))
        { // Lose one defense
            ApplyStatusEffect(BattleManager.StatusEffectEnum.defence, -1);
        }
        // Do we have any engaged enemies?
        if (engagedEnemies.Count == 0)
        {
            // We need to lose 1 momentum and insight.
            if (statusEffects.ContainsKey(BattleManager.StatusEffectEnum.momentum))
            { // Lose one momentum
                ApplyStatusEffect(BattleManager.StatusEffectEnum.momentum, -1);
            }
            if (statusEffects.ContainsKey(BattleManager.StatusEffectEnum.insight))
            { // Lose one insight
                ApplyStatusEffect(BattleManager.StatusEffectEnum.insight, -1);
            }
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

        if (statusEffects.TryGetValue(status, out var val))
        {
            Debug.Log("We now have " + val.EffectValue + " power");
        }
    }

    // Right now, this is called by a button.
    public void GetCardReward()
    {
        puim.OpenCardRewardView();
    }

    // Returns the damage bonus from momentum
    internal int GetMomentumBonus()
    {
        if (statusEffects.TryGetValue(BattleManager.StatusEffectEnum.momentum, out var val))
        {
            return val.EffectValue;
        }
        else
            return 0;
    }

    // Returns damage bonus from insight, then removes it.
    internal float GetInsightBonus()
    {
        if (statusEffects.TryGetValue(BattleManager.StatusEffectEnum.insight, out var val))
        {
            int returnVal = val.EffectValue;
            // Remove our status
            ApplyStatusEffect(BattleManager.StatusEffectEnum.insight, -1 * returnVal);

            return returnVal + 1f;
        }
        else
            return 1f;
    }
}
