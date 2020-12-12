using Roguelike;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This goes onto the player holder.
public class PlayerController : TileCreature
{
    private const int movementEnergyCost = 1;
    private const float cardSpiritCostMult = 1.4f;

    public Rigidbody rb;
    public BoxCollider boxCollider;
    public PlayerAnimation Animation;

    // Card Management
    public static Deck playerDeck;
    public PlayerUIManager puim; // This class manages the player's ui graphically.

    private int cardRedrawAmount = 8; // Redraw up to this number of cards.
    private int maxHandSize = 12;
    private int maxTurnsUntilDraw = 1; // Draw a card every X turns.
    private int turnsUntilDraw = 1;

    // Resources
    private int currentEnergy;
    private int energyPerTurn = 3;

    private float currentSpirit;
    private int maxSpirit = 1000;
    private int spiritCostPerDiscard = 30; // How much spirit it costs to discard a card
    private int health;
    private int maxHealth = 100;
    private enum SpiritLossStage { full, stage1, stage2, empty };
    private SpiritLossStage spiritStage = SpiritLossStage.full;

    private int money;

    public BloodSplatter Splatter;

    // LOS
    public bool[,] LoSGrid; // A grid that shows which tiles the player has LOS to relative to themselves. Used for un-obscuring the camera.
    public bool[,] SimpleLoSGrid; // Uses simple middle-to-middle LoS. Used for non-corner cutting attacks.

    // Status effects

    private List<TileEntity> engagedEnemies;
    public float CurrentSpirit
    {
        get { return currentSpirit; }
        private set { currentSpirit = value; puim.SetSpiritPercentage(currentSpirit / maxSpirit); } // Automatically update the display whenever spirit changes
    }
    public int CurrentEnergy
    {
        get { return currentEnergy; }
        private set { currentEnergy = value; puim.SetCurrentEnergy(currentEnergy); }
    }
    public int Health
    {
        get { return health; }
        private set { health = value; puim.SetCurrentHealth(health, maxHealth); }
    }
    public int Money
    {
        get { return money; }
        private set { money = value; puim.SetCurrentMoney(money); }
    }

    public bool IsFacingRight = true;
    public SpriteRenderer Sprite;

    void Awake()
    {
    }

    // Things to set once everything else has been initialized
    private void Start()
    {
        AssignVariables();
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
        boxCollider = GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        puim = GetComponent<PlayerUIManager>();

        // Cards
        AssignInitialDeck();

        // Resources
        CurrentEnergy = energyPerTurn;
        CurrentSpirit = maxSpirit;
        Health = maxHealth;
        Money = 0;

        UpdateLOS();


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
        Card slash = CardFactory.GetCard("slash");
        slash.Owner = this;
        Card cinders = CardFactory.GetCard("cinders");
        cinders.Owner = this;
        Card footwork = CardFactory.GetCard("Footwork");
        footwork.Owner = this;
        Card dragonheart = CardFactory.GetCard("dragonheart");
        dragonheart.Owner = this;
        playerDeck.InsertCardAtEndOfDrawPile(slash);
        playerDeck.InsertCardAtEndOfDrawPile(slash);
        playerDeck.InsertCardAtEndOfDrawPile(slash);
        playerDeck.InsertCardAtEndOfDrawPile(slash);
        playerDeck.InsertCardAtEndOfDrawPile(slash);
        playerDeck.InsertCardAtEndOfDrawPile(footwork);
        playerDeck.InsertCardAtEndOfDrawPile(footwork);
        playerDeck.InsertCardAtEndOfDrawPile(cinders);
        playerDeck.InsertCardAtEndOfDrawPile(cinders);
        playerDeck.InsertCardAtEndOfDrawPile(dragonheart);

        if (false) // Debugging testing certain cards.
        {
            Card card1 = CardFactory.GetCard("celerity");
            card1.Owner = this;
            playerDeck.InsertCardAtEndOfDrawPile(card1);
            playerDeck.InsertCardAtEndOfDrawPile(card1);

            Card card2 = CardFactory.GetCard("sagacity");
            card2.Owner = this;
            playerDeck.InsertCardAtEndOfDrawPile(card2);
            playerDeck.InsertCardAtEndOfDrawPile(card2);

            Card card3 = CardFactory.GetCard("blockade");
            card3.Owner = this;
            playerDeck.InsertCardAtEndOfDrawPile(card3);
            playerDeck.InsertCardAtEndOfDrawPile(card3);
        }


        playerDeck.ShuffleDeck();
        DrawToHandLimit();
    }

    /// <summary>
    /// Adds the selected card to the discard pile.
    /// </summary>
    /// <param name="cardData">Card to add</param>
    internal void GainCard(Card cardData, bool addToHand)
    {
        cardData.Owner = this;
        if (addToHand)
        {
            if (playerDeck.hand.Count >= maxHandSize)
            {
                puim.ShowAlert("You have too many cards in hand. Adding to discard instead.");
                playerDeck.discardPile.Add(cardData);
            }
            else
            {
                playerDeck.AddCardToHand(cardData);
                puim.SpawnCardInHand(cardData);
            }
        }
        else
            playerDeck.discardPile.Add(cardData);
    }

    public void GainInventoryCard(Card card)
    {
        card.Owner = this;
        playerDeck.AddCardToInventory(card);
        puim.CardAddedToInventory(card);
    }

    public void TriggerCardReward(Card cardData)
    {
        // Gain a card reward from the card reward screen.
        GainCard(cardData, true);
        puim.LeaveCardRewardScreen(true);
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
                    puim.SetTurnsUntilDrawText(turnsUntilDraw);
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
            puim.ShowAlert("You have too many cards to draw more.");
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
        CurrentEnergy += amount;
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

    public enum AttemptToPlayCardFailReasons { success, notPlayerTurn, notEnoughEnergy, bossInvincible };
    // Attempts to play that card index on given tile.
    public AttemptToPlayCardFailReasons AttemptToPlayCard(CardInterface cardToPlay, Vector2Int target)
    {
        // Check if we can play card
        if (!TurnManager.instance.CanPlayCard())
            return AttemptToPlayCardFailReasons.notPlayerTurn;

        // Do we have the energy for it?
        if (cardToPlay.cardData.CardInfo.EnergyCost > CurrentEnergy)
        {
            return AttemptToPlayCardFailReasons.notEnoughEnergy;
        }

        var tile = BattleGrid.instance.CurrentFloor.map[target.x, target.y];
        var boss = tile.GetEntityOnTile() as BossBody;
        if (boss != null && boss.Invincible)
        {
            return AttemptToPlayCardFailReasons.bossInvincible;
        }

        // Play the card
        TriggerCardPlay(cardToPlay, target);
        // Discarding/banishing is handled in ResolveCardPlayed()
        //playerDeck.DiscardCardAtIndex(index);
        return AttemptToPlayCardFailReasons.success;
    }

    // Plays the relevant card. Does not discard it afterwards.
    private void TriggerCardPlay(CardInterface cardToPlay, Vector2Int target, bool resetTrackerWhenDone = true)
    {
        // Start card tracking
        BattleManager.instance.StartCardTracking(cardToPlay);

        // Pay the cost of the card
        PayCardCost(cardToPlay.cardData);

        // Activate its effects.
        cardToPlay.cardData.Activate(BattleManager.ConvertVector(transform.position), target);

        if (resetTrackerWhenDone)
            ResolveCardPlayed();
    }

    // Called at the end of a stack of card effects.
    // Determines where the card goes afterwards as well.
    private void ResolveCardPlayed()
    {
        // If we used momentium, clear all our momentium.
        int momentumUsed = BattleManager.cardResolveStack.QueryUsedMomentum();
        if (momentumUsed > 0)
        {
            if (statusEffects.ContainsKey(BattleManager.StatusEffectEnum.momentum))
            {
                ApplyStatusEffect(BattleManager.StatusEffectEnum.momentum, -1 * momentumUsed); // Removes momentium
            }
        }

        // Determine where the card should go.
        if (BattleManager.cardResolveStack.GetCurrentlyResolvingCard().GetLocation() == CardInterface.CardInterfaceLocations.inventory)
        { // Destroy the card from the inventory.
            RemoveCardFromInventory(BattleManager.cardResolveStack.GetCurrentlyResolvingCard().cardIndex);
        }
        else
        {
            //while (BattleManager.cardResolveStack.GetCurrentlyResolvingCard() != null)
            // {
            switch (BattleManager.cardResolveStack.GetCurrentlyResolvingCard().cardData.CardInfo.ResolveBehavior)
            {
                case CardInfo.ResolveBehaviorEnum.discard:
                    // Discard the resolved card
                    DiscardCardIndex(BattleManager.cardResolveStack.GetCurrentlyResolvingCard().cardIndex);
                    break;
                case CardInfo.ResolveBehaviorEnum.banish:
                    BanishCardIndex(BattleManager.cardResolveStack.GetCurrentlyResolvingCard().cardIndex, BattleManager.cardResolveStack.GetCurrentlyResolvingCard().cardData.CardInfo.BanishAmount);
                    break;
            }
            // cardResolveStack.PopCard(); }
            // These comments are here for a guideline of what to do if we ever switch over to a proper card stack.
        }
        BattleManager.instance.StopCardTracking();
    }

    // Deducts the energy and spirit cost of the card.
    private void PayCardCost(Card cardData)
    {
        CurrentEnergy -= cardData.CardInfo.EnergyCost;
        LoseSpirit(cardData.CardInfo.SpiritCost * cardSpiritCostMult);
    }

    // Discards this card and fixes the indexes for the others.
    public void DiscardCardIndex(int index, bool fromEffect = false)
    {
        // Discard card data
        playerDeck.DiscardCardAtIndex(index, fromEffect);
        puim.DestroyCardAtIndex(index);
    }

    public void BanishCardIndex(int index, int amount)
    {
        playerDeck.BanishCardAtIndex(index, amount);
        puim.DestroyCardAtIndex(index);
    }

    // Called by the puim when the player forcibly tries to discard a card
    public void ManuallyDiscardCardAtIndex(int index)
    {
        playerDeck.DiscardCardAtIndex(index);
        puim.DestroyCardAtIndex(index);
        LoseSpirit(spiritCostPerDiscard);
    }

    public void RemoveCardFromInventory(int index)
    {
        playerDeck.RemoveInventoryCardAtIndex(index);
        puim.DestroyInventoryCardAtIndex(index);
    }

    // Update is called once per frame
    void Update()
    {
        // Hitting P will toggle control camera
        if (Input.GetKeyDown(KeyCode.P))
            if (puim.IsStateControllingCamera())
                puim.MoveToState(PlayerUIManager.PlayerUIState.standardCardDrawer);
            else
                puim.MoveToState(PlayerUIManager.PlayerUIState.controllingCamera);
        // Cheat key to give you gold.
        else if (Input.GetKeyDown(KeyCode.F1))
        {
            Money += 30;
        }
        // Press C to open up Card view.
        else if (Input.GetKeyDown(KeyCode.C))
        {
            puim.ToggleShowMassCardView();
        }
        // Press B to Buy a card.
        else if (Input.GetKeyDown(KeyCode.B))
        {
            GetCardReward(30);
        }
        // Press V to open inVentory
        else if (Input.GetKeyDown(KeyCode.V))
        {
            puim.ToggleInventoryVisible();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            Card hanaFuda = CardFactory.GetCardTheme("hanafuda").GetRandomCardInTheme();
            hanaFuda.Owner = this;
            GainInventoryCard(hanaFuda);
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            GainSpiritPercentage(.05f);
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            LoseSpirit(75);
        }
    }


    /// <summary>
    /// Handles moving the player. Returns true if the player moves by button click
    /// </summary>
    /// <returns></returns>
    public bool HandleMovement()
    {
        bool movementLocked = puim.IsStateMovementLocked();
        if (!movementLocked)
        {
            return HandleMovementInput(); // Get input and pick somewhere to move.
        }
        else if (puim.IsStateControllingCamera())
        {
            Debug.Log("HandleCameraMovement");
            puim.HandleCameraMovement();
        }
        return false;

    }

    /// <summary>
    /// Handle user input. Returns true if the player moves from the input
    /// </summary>
    /// <returns></returns>
    private bool HandleMovementInput()
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
                    return false;
            }
            Vector2Int movementVector = new Vector2Int(xDir + xPos, zDir + zPos);

            if (CheckCanMoveInDirection(xDir, zDir))
            {
                if (IsFacingRight && xDir < 0)
                {
                    Sprite.flipX = true;
                    IsFacingRight = false;
                }
                else if (!IsFacingRight && xDir > 0)
                {
                    Sprite.flipX = false;
                    IsFacingRight = true;
                }

                MoveToPosition(movementVector, speedOfMovement);
                // Recalculate LOS
                UpdateLOS(movementVector);

                return true;
            }
        }

        return false;
    }



    // Checks the target tile for anything that activates when you move onto it. EG: items, terrain
    // Returns true if we should still move.
    private bool ActivateMoveOntoEffects(Vector2Int newMoveTarget)
    {
        var tile = BattleManager.instance.GetTileAtLocation(newMoveTarget.x, newMoveTarget.y);

        //Pick up any money that we ended our turn on top of
        var itemType = tile.tileItemType;

        switch (itemType)
        {
            case Tile.TileItemType.money:
                DroppedMoney moneyobj = tile.GetItemOnTile() as DroppedMoney;
                if (moneyobj != null)
                {
                    Debug.Log("Picked up money");
                    AudioManager.PlayPickMoney();
                    Money += moneyobj.Value;
                    moneyobj.DestroySelf();
                    tile.tileItemType = Tile.TileItemType.empty;
                }
                break;
            case Tile.TileItemType.smallChest:
                TreasureChest treasureObj = tile.GetItemOnTile() as TreasureChest;
                if (treasureObj != null)
                { // loot boxes babbbyyyyyyyyy
                    int randomTreasurePull = UnityEngine.Random.Range(0, 4);
                    switch (randomTreasurePull)
                    {
                        case 0: // coins
                            int randomAmountOfCoins = UnityEngine.Random.Range(18, 37);
                            puim.ShowAlert("You found " + randomAmountOfCoins + " coins in the chest.");
                            Money += randomAmountOfCoins;
                            break;
                        case 1:
                            puim.ShowAlert("You found a card reward in this chest.");
                            GetCardReward(0);
                            break;
                        case 2:
                        case 3:
                            Card hanaFuda = CardFactory.GetCardTheme("hanafuda").GetRandomCardInTheme();
                            hanaFuda.Owner = this;
                            GainInventoryCard(hanaFuda);
                            puim.ShowAlert("You found the hanfuda card \"" + hanaFuda.CardInfo.Name + "\". Press V to open your inventory.");
                            break;
                    }
                    treasureObj.DestroySelf();
                    tile.tileItemType = Tile.TileItemType.empty;
                }
                break;
        }


        if (tile.tileTerrainType == Tile.TileTerrainType.trap)
        {
            var trap = tile.terrainOnTile as Trap;
            if (trap != null)
            {
                trap.Activate();
            }
        }
        else if (tile.tileTerrainType == Tile.TileTerrainType.stairsDown)
        {
            GoDownStairsEffects();
            BattleGrid.instance.GoDownFloor();
            TurnManager.instance.CurrentPhase = TurnManager.TurnPhase.start;
            TurnManager.instance.CurrentTurn = TurnManager.WhoseTurn.player;
        }



        return true;
    }

    public override void MoveToPosition(Vector2Int destination, float timeToMove)
    {
        Animation.StartWalkingAnimation();
        base.MoveToPosition(destination, timeToMove);
    }

    protected override void OnStopMoving()
    {
        ActivateMoveOntoEffects(new Vector2Int(xPos, zPos));
        Animation.StartIdleAnimation();

        CheckForTraps();
    }

    //Checks the area around the player for traps
    private void CheckForTraps()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                var tile = BattleGrid.instance.CurrentFloor.map[xPos + i, zPos + j];
                if (tile.tileTerrainType == Tile.TileTerrainType.trap)
                {
                    var trap = tile.terrainOnTile as Trap;
                    if (trap != null && Random.RandBool(trap.ProbOfBeingSeen))
                    {
                        trap.MakeVisible();
                    }
                }
            }
        }
    }

    // Checks to see if we can move in that direction.
    // Uses the battlegrid.
    private bool CheckCanMoveInDirection(int xDir, int zDir)
    {
        // Do we have enough energy?
        if (currentEnergy < movementEnergyCost)
        {
            puim.ShowAlert("Not enough energy. Press enter or q to end turn.");
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

    public void GoDownStairsEffects()
    {
        // Decrement banished cards.
        playerDeck.TickDownBanishedCards(0);

        // Gain back 35% spirit.
        GainSpiritPercentage(.35f);

        // Heal for 20 HP
        TakeDamage(Vector2Int.zero, -20);
    }

    // Applies incoming damage
    public override int TakeDamage(Vector2Int locationOfAttack, int damage)
    {
        int oldHealth = Health;
        if (damage >= 0) // Damage
        {
            // Do we have defense?
            if (statusEffects.TryGetValue(BattleManager.StatusEffectEnum.defense, out StatusEffectDataHolder val))
            {
                int oldDamage = damage;
                damage -= val.EffectValue;
                ApplyStatusEffect(BattleManager.StatusEffectEnum.defense, -1 * oldDamage); // Deal damage to defense
            }

            if (damage > 0)
            {
                AudioManager.PlayPlayerHit();
                Splatter.Play(locationOfAttack);
                StartCoroutine(HitColoration());
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

        return (oldHealth - Health);
    }

    private IEnumerator HitColoration(float timeToWait = .05f)
    {
        Sprite.material.shader = BattleManager.instance.ShaderGUItext;
        Sprite.color = Color.white;
        yield return new WaitForSeconds(timeToWait);
        Sprite.material.shader = BattleManager.instance.ShaderSpritesDefault;
        Sprite.color = Color.white;
    }

    int loseSpiritEveryX = 3;
    int walkCounter = 0;
    // Handles stuff that happens at the end of the player turn.
    public void EndOfTurn()
    {
        //Debug.Log("Player Controller end of turn");
        // Spirit decay
        walkCounter++;
        if (walkCounter >= loseSpiritEveryX)
        {
            walkCounter = 0;
            LoseSpirit(1);
        }
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

    int turnsSinceInCombat = 0;
    int turnsUntilStatusDecay = 3;
    private void StartOfTurnStatusDecay()
    {
        // Do we have defense?
        if (statusEffects.ContainsKey(BattleManager.StatusEffectEnum.defense))
        { // Lose one defense
            ApplyStatusEffect(BattleManager.StatusEffectEnum.defense, -1);
        }
        // Do we have any engaged enemies?
        if (engagedEnemies.Count == 0)
        {
            // We're not in combat.
            turnsSinceInCombat++;
            if (turnsSinceInCombat >= turnsUntilStatusDecay)
            {
                // We need to lose 1 momentum and insight.
                if (statusEffects.ContainsKey(BattleManager.StatusEffectEnum.momentum))
                { // Lose one momentum
                    ApplyStatusEffect(BattleManager.StatusEffectEnum.momentum, -1);
                }
                if (statusEffects.ContainsKey(BattleManager.StatusEffectEnum.insight))
                { // Lose one insight
                    Debug.Log("Losing insight. Engaged enemy count: " + engagedEnemies.Count);
                    ApplyStatusEffect(BattleManager.StatusEffectEnum.insight, -1);
                }
            }
        }
        else
        {
            turnsSinceInCombat = 0;
        }
    }

    // Causes a loss of this much spirit. Applies reductions from low spirit.
    // Lose .5% less per 1% lost.
    public void LoseSpirit(float amount)
    {
        float lossMult = 1 - (0.5f * (1 - (CurrentSpirit / maxSpirit)));
        //Debug.Log("Currently have " + CurrentSpirit.ToString("0.00") + "/" + maxSpirit + " spirit, which makes the loss mult " + lossMult);
        float amountToLose = amount * lossMult;
        CurrentSpirit -= amountToLose;
        if (CurrentSpirit < 0)
        {
            CurrentSpirit = 0;
            int damageToTake = Math.Max(((int)amountToLose) / 2, 1);
            TakeDamage(new Vector2Int(xPos, zPos), damageToTake);
        }
        UpdateSpiritStage();
    }

    public void GainSpirit(float amount)
    {
        CurrentSpirit += amount;
        if (CurrentSpirit > maxSpirit)
            CurrentSpirit = maxSpirit;
        UpdateSpiritStage();
    }

    public void GainSpiritPercentage(float percent)
    {
        float amount = maxSpirit * percent;
        if (amount > 0)
            GainSpirit(amount);
        else
            LoseSpirit(-1 * amount);
    }

    // Updates the penalties from low spirit.
    private void UpdateSpiritStage()
    {
        float spiritPercentage = CurrentSpirit / maxSpirit;
        if (spiritPercentage >= .67 && spiritStage != SpiritLossStage.full)
        {
            LeaveSpiritStage();
            EnterSpiritStage(SpiritLossStage.full);
        }
        else if (spiritPercentage >= .33 && spiritPercentage < .67 && spiritStage != SpiritLossStage.stage1)
        {
            LeaveSpiritStage();
            EnterSpiritStage(SpiritLossStage.stage1);
        }
        else if (spiritPercentage > 0 && spiritPercentage < .33 && spiritStage != SpiritLossStage.stage2)
        {
            LeaveSpiritStage();
            EnterSpiritStage(SpiritLossStage.stage2);
        }
        else if (spiritPercentage == 0 && spiritStage != SpiritLossStage.empty)
        {
            LeaveSpiritStage();
            EnterSpiritStage(SpiritLossStage.empty);
        }
    }

    private void LeaveSpiritStage()
    {
        // Remove any stacks of spirit loss we have.
        if (statusEffects.ContainsKey(BattleManager.StatusEffectEnum.spiritLoss))
        {
            StatusEffectDataHolder x = statusEffects[BattleManager.StatusEffectEnum.spiritLoss];
            puim.RemoveStatusEffect(x);
            statusEffects.Remove(BattleManager.StatusEffectEnum.spiritLoss);
        }

        // Reset other penalties to normal.
        int spStage = (int)spiritStage;
        Debug.Log("Leaving stage " + spStage);
        if (spStage >= 1) // Mild loss. .33 - .67
        {
            // Reset redraw limit penalty
            cardRedrawAmount += 1;
            // Reset draw times.
            maxTurnsUntilDraw -= 1;
        }
        
        if (spStage >= 2) // Severe loss. 0-.33
        {
            cardRedrawAmount += 1;
            maxTurnsUntilDraw -= 1;
        }

        Debug.Log("Card redraw is " + cardRedrawAmount);

    }

    private void EnterSpiritStage(SpiritLossStage e)
    {
        int spStage = (int)e;
        if (spStage >= 1) // Mild loss. .33 - .67
        {
            ApplyStatusEffect(BattleManager.StatusEffectEnum.spiritLoss, 1);
            cardRedrawAmount -= 1;
            maxTurnsUntilDraw += 1;
        }

        if (spStage >= 2) // Severe loss. 0-.33
        {
            ApplyStatusEffect(BattleManager.StatusEffectEnum.spiritLoss, 1);
            cardRedrawAmount -= 1;
            maxTurnsUntilDraw += 1;
        }

        if (spStage == 3) // Empty.
        {
            ApplyStatusEffect(BattleManager.StatusEffectEnum.spiritLoss, 1);
        }
        spiritStage = e;
    }

    // Returns true on a success
    public bool RemoveEngagedEnemy(TileEntity tar)
    {
        return engagedEnemies.Remove(tar);
    }

    public void AddEngagedEnemy(TileEntity tar)
    {
        Debug.Log("Enemy is attacking");
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
        Debug.Log("Status effect applied: " + status.ToString() + " w/ power " + amount);
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
    public void GetCardReward(int price)
    {
        if (!puim.CanGetCardReward())
        {
            return;
        }
        if (price > 0)
        {
            if (Money < price)
            {
                puim.ShowAlert("Need to pay " + price + " for a card.");
                return;
            }
            else
            {
                Money -= price;
            }
        }

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

    public void GainMaxHP(int amount)
    {
        maxHealth += amount;
        Health += amount;
    }

    public void GainMoney(int amount)
    {
        Money += amount;
    }    
}
