﻿using Cinemachine;
using Roguelike;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This class keeps track of the graphical hand.

public class PlayerUIManager : MonoBehaviour
{
    public GameObject baseUIObject;
    public TextMeshProUGUI energyTextComponent;
    public TextMeshProUGUI energyPerTurnTextComponent;
    public TextMeshProUGUI healthTextComponent;
    public Image spiritFillImage;
    public Image healthFillImage;
    public TextMeshProUGUI spiritTextComponent;
    private HorizontalLayoutGroup handLayout;
    private GridLayoutGroup statusEffectLayout;
    private GridLayoutGroup massCardViewGridLayout;
    private Image massCardViewBackground;
    [SerializeField] private GameObject cardRewardViewbackground;
    private HorizontalLayoutGroup cardRewardHorizontalLayout;

    // Ones that should be assigned in the editor.
    [SerializeField] private TextMeshProUGUI moneyDisplayLabel = null;
    [SerializeField] private GameObject backShading = null;
    [SerializeField] private GameObject banishCounter = null;

    // Player UI FSM.
    public enum PlayerUIState { standardCardDrawer, controllingCamera, massCardView, cardRewardView, inEventChoice }

    internal class PlayerUIStateClass
    {
        public bool tooltipOpen;
        public bool inventoryOpen;
        public PlayerUIState internalUIState = PlayerUIState.standardCardDrawer;

        public PlayerUIStateClass(PlayerUIState initial)
        {
            internalUIState = initial;
            tooltipOpen = false;
            inventoryOpen = false;
        }
    }

    private PlayerUIStateClass playerUIState = new PlayerUIStateClass(PlayerUIState.standardCardDrawer);

    // Tooltips
    [SerializeField] private GameObject tooltipView;
    [SerializeField] private GameObject tooltipCardHolder;
    [SerializeField] private GameObject tooltipContent;

    // Inventory
    [SerializeField] private GameObject inventoryView;
    [SerializeField] private GameObject inventoryContent;
    private List<CardInterface> graphicalInventory;

    // PickChoiceView
    [SerializeField] private GameObject pickChoiceView;
    [SerializeField] private TextMeshProUGUI pickChoiceTextDescription;

    private PlayerController pc;

    // Prefabs
    public GameObject cardPrefab;
    public GameObject tileSelectionPrefab;
    public GameObject tileHighlightPrefab; // Used to highlight which tiles you can play cards on.
    [HideInInspector] private List<GameObject> spawnedTileHighlightPrefabs;
    public GameObject statusEffectIconPrefab;

    private List<CardInterface> graphicalHand; // The cards graphically
    private FadeObjectScript notificationBarFade; // The fade script for the text just above the hand.

    // Selected Tile
    [HideInInspector] private GameObject spawnedTileSelectionPrefab;
    private Vector2Int selectedTileGridCoords;
    [HideInInspector] public bool tileCurrentlySelected = false;
    //[HideInInspector] public Vector3 selectedTile;

    // Stuff to detect wether the player is clicking on the UI
    //public GraphicRaycaster raycaster; // These need to be set to the ones on the main canvas.
    //public PointerEventData pointerEventData;
    //public EventSystem eventSystem; Removed in the switch over to DOOZY.
    private float initialClickTime = 0f; // Time when the player clicked on a card
    private float doubleClickTime = 0.3f; // How fast the player has to click to register as a double click.

    public CardInterface selectedCard { get; set; }
    public bool[,] selectedCardRange;

    // Camera
    private CinemachineVirtualCamera vcam;

    public void Awake()
    {
        // Assign game objects if we haven't done it in the editor.

        if (baseUIObject == null)
            baseUIObject = GameObject.Find("Canvas - MasterCanvas");

        string[] objNames = new string[] {"CurrentEnergyText", "EnergyPerTurnText", "HealthTextObject", "SpiritBarFill", "SpiritTextObject", "HandLayout", "StatusEffectGridLayout",
                                            "DeckViewContent", "View - MassCardView", "CardOptionsHolder", "HealthBarFill"};
        Type[] typeNames = new Type[] {typeof(TextMeshProUGUI), typeof(TextMeshProUGUI), typeof(TextMeshProUGUI), typeof(Image), typeof(TextMeshProUGUI), typeof(HorizontalLayoutGroup),
        typeof (GridLayoutGroup), typeof (GridLayoutGroup), typeof(Image), typeof(HorizontalLayoutGroup), typeof(Image)};
        Component[] valArray = BattleManager.RecursiveVariableAssign(baseUIObject, objNames, typeNames);

        energyTextComponent = valArray[0] as TextMeshProUGUI;
        energyPerTurnTextComponent = valArray[1] as TextMeshProUGUI;
        healthTextComponent = valArray[2] as TextMeshProUGUI;
        spiritFillImage = valArray[3] as Image;
        spiritTextComponent = valArray[4] as TextMeshProUGUI;
        handLayout = valArray[5] as HorizontalLayoutGroup;
        statusEffectLayout = valArray[6] as GridLayoutGroup;
        massCardViewGridLayout = valArray[7] as GridLayoutGroup;
        massCardViewBackground = valArray[8] as Image;
        cardRewardHorizontalLayout = valArray[9] as HorizontalLayoutGroup;
        healthFillImage = valArray[10] as Image;

        graphicalHand = new List<CardInterface>();
        graphicalInventory = new List<CardInterface>();
        pc = GetComponent<PlayerController>();

        spawnedTileSelectionPrefab = Instantiate(tileSelectionPrefab);
        spawnedTileSelectionPrefab.SetActive(false);

        selectedCard = null;

        spawnedTileHighlightPrefabs = new List<GameObject>();

        if (notificationBarFade == null)
            notificationBarFade = GameObject.Find("NotificationText").GetComponent<FadeObjectScript>();

        vcam = GameObject.Find("CM vcam1").GetComponent<CinemachineVirtualCamera>();
    }

    private void Update()
    {

        // Has the player clicked?
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // UI objects should be buttons
            }
            else
            {
                HandleWorldClick();
            }
        }
    }

    // Debugging method for printing a grid of bools to the debug log.
    public static void PrintGrid(bool[,] grid)
    {
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            string printMessage = "";
            for (int j = 0; j < grid.GetLength(1); j++)
            {
                if (grid[i, j])
                    printMessage += "T ";
                else
                    printMessage += "X ";
            }
            Debug.Log(printMessage);
        }
    }

    // This handles a click that doesn't hit any UI elements.
    // Currently, this just handles selecting a world tile.
    private void HandleWorldClick()
    {
        // We want to cast a line from the camera onto the ground plane and see what it collides with in order to see which tile the player clicks.
        Plane playerPlane = new Plane(Vector3.up, BattleManager.player.transform.position);
        Ray ray = UnityEngine.Camera.main.ScreenPointToRay(Input.mousePosition);
        //RaycastHit hit;

        if (playerPlane.Raycast(ray, out float hitDistance))
        {
            Vector3 mousePosition = ray.GetPoint(hitDistance);
            // Want to spawn tileSelectionPrefab around here.

            // I used to have logic here for wether the values here are negative. But now everything should exist in the (+,+) quadrant in unity space.
            int spawnX = (int)(mousePosition.x + 0.5f);
            int spawnZ = (int)(mousePosition.z + 0.5f);
            Vector2Int gridPos = new Vector2Int(spawnX, spawnZ);
            /*if (mousePosition.x >= 0)
                spawnX = (int)mousePosition.x + 0.5f;
            else
                spawnX = (int)mousePosition.x - 0.5f;

            if (mousePosition.z >= 0)
                spawnZ = (int)mousePosition.z + 0.5f;
            else
                spawnZ = (int)mousePosition.z - 0.5f;*/

            Vector3 newSpawnLocation = BattleManager.ConvertVector(gridPos, BattleManager.player.transform.position.y);
            // Is this already selected?
            if (tileCurrentlySelected && spawnedTileSelectionPrefab.transform.position.Equals(newSpawnLocation))
                DeselectTile();
            else
                SelectTile(newSpawnLocation);

        }
    }
    #region CardSelection
    // Graphically spawns the card drawnCard into the player's hand.
    internal void SpawnCardInHand(Card drawnCard)
    {
        //handLayout.SetLayoutHorizontal();
        GameObject newCard = GameObject.Instantiate(cardPrefab, handLayout.transform);

        CardInterface ci = newCard.GetComponent<CardInterface>();
        ci.AssignTextMeshVariables();
        ci.cardData = drawnCard;
        ci.cardIndex = graphicalHand.Count;
        graphicalHand.Add(ci);
        ci.OnCardSpawned(CardInterface.CardInterfaceLocations.hand);

        selectedCard = null;

        UpdateHandLayoutSpacing();
    }

    // Note that this destroys the card graphically. It does not alter anything about the data.
    internal void DestroyCardAtIndex(int index)
    {
        if (selectedCard && selectedCard.cardIndex == index && selectedCard.GetLocation() == CardInterface.CardInterfaceLocations.hand)
        {
            // If we're discarding the selected card, deselect it.
            DeselectCard();
        }
        // Loop through and reduce the index of other cards by one
        for (int i = index + 1; i < graphicalHand.Count; i++)
        {
            graphicalHand[i].cardIndex--;
        }

        // Destroy the gameobject
        GameObject.Destroy(graphicalHand[index].gameObject);

        // Now remove
        graphicalHand.RemoveAt(index);

        // Update spacing
        UpdateHandLayoutSpacing();
    }

    /// <summary>
    /// Removes all of the cards from the UI
    /// </summary>
    public void ClearCards()
    {
        if (selectedCard)
        {
            DeselectCard();
        }
        for (int i = graphicalHand.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(graphicalHand[i].gameObject);
        }
        graphicalHand.Clear();

        // Update spacing
        UpdateHandLayoutSpacing();
    }


    // Called when a card in the hand is clicked.
    internal void CardInHandClicked(int index)
    {
        Debug.Log("Card clicked: " + index + ", selectedCard = " + selectedCard);
        // We're only allowed to click cards in standard UI state.
        if (IsStateMovementLocked())
        {
            return;
        }

        // Is this a double click?
        if (selectedCard && Time.time - initialClickTime < doubleClickTime && selectedCard.cardIndex == index && selectedCard.GetLocation() == CardInterface.CardInterfaceLocations.hand)
        {
            // Yes. Double click
            Debug.Log("Double click on card " + index);

            // Inform the player controller.
            pc.ManuallyDiscardCardAtIndex(index);
            return;
        }
        else
            initialClickTime = Time.time;

        // If player has clicked on highlighted card, unhighlight it
        if (selectedCard != null && index == selectedCard.cardIndex && selectedCard.GetLocation() == CardInterface.CardInterfaceLocations.hand)
        {
            DeselectCard();
        }
        else
        {
            // Unhighlight all other cards, highlights this one.
            DeselectCard();
            SelectCard(index, CardInterface.CardInterfaceLocations.hand);
        }
    }

    internal void SelectCard(int index, CardInterface.CardInterfaceLocations loc)
    {

        if (loc == CardInterface.CardInterfaceLocations.hand)
        {
            Debug.Log("Selecting hand card number " + index);
            graphicalHand[index].Highlight();
            selectedCard = graphicalHand[index];
            selectedCardRange = GenerateCardRange(graphicalHand[index].cardData);
        }
        else
        {
            Debug.Log("Selecting hand card number " + index + ", size of inventory is " + graphicalInventory.Count);
            graphicalInventory[index].Highlight();
            selectedCard = graphicalInventory[index];
            selectedCardRange = GenerateCardRange(graphicalInventory[index].cardData);
        }

        // Now we need to highlight all the squares where this can be played.


        SpawnHighlightTiles(selectedCardRange);
    }

    // Deselects whatever card we have selected
    private void DeselectCard()
    {
        if (selectedCard != null)
        {
            if (selectedCard.GetLocation() == CardInterface.CardInterfaceLocations.hand)
            {
                if (selectedCard.cardIndex < graphicalHand.Count)
                {
                    selectedCard.DisableHighlight();
                }
            }
            else if (selectedCard.GetLocation() == CardInterface.CardInterfaceLocations.inventory)
            {
                if (selectedCard.cardIndex < graphicalInventory.Count)
                {
                    selectedCard.DisableHighlight();
                }
            }

            selectedCard = null;
            DestroyHighlightTiles();
        }
    }

    #endregion

    #region TileSelection
    public void DeselectTile()
    {
        // If we've selected an enemy, hide its canvas
        if (BattleManager.IsVectorNonNeg(selectedTileGridCoords) && BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].tileEntityType == Tile.TileEntityType.enemy)
            ((EnemyBody)BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].GetEntityOnTile()).EnemyUI.HideHealthBar(0.3f, 1f);
        selectedTileGridCoords = Vector2Int.one * -1;
        tileCurrentlySelected = false;
        spawnedTileSelectionPrefab.SetActive(false);
    }

    public void SelectTile(Vector3 target)
    {
        Vector2Int tile = BattleManager.ConvertVector(target);
        // If we have a card selected, try to play that card
        if (selectedCard != null)
        {
            // Convert this vector3 into an offset from the player.
            int xLength = selectedCardRange.GetLength(0);
            int zLength = selectedCardRange.GetLength(1);
            int xArrayIndex = (int)(target.x - pc.transform.position.x) + xLength / 2;
            int zArrayIndex = (int)(target.z - pc.transform.position.z) + zLength / 2;
            Debug.Log("xOffset is " + xArrayIndex + ", zOffset is " + zArrayIndex);

            if ((xArrayIndex >= 0 && xArrayIndex < xLength && zArrayIndex >= 0 && zArrayIndex < zLength) && selectedCardRange[xArrayIndex, zArrayIndex])
            {
                Debug.Log("Attempting to play card on " + tile.x + "," + tile.y);
                CheckTilePlayConditions(selectedCard, tile);
            }
            else
            {
                ShowAlert("Selected card cannot be played on that tile");
            }
        }
        else
        {
            // Check to make sure player has clicked within selectable area.
            if (BattleManager.IsVectorNonNeg(tile) && tile.x < BattleGrid.instance.map.GetLength(0) && tile.y < BattleGrid.instance.map.GetLength(1))
            {
                DeselectTile();
                tileCurrentlySelected = true;
                spawnedTileSelectionPrefab.SetActive(true);
                spawnedTileSelectionPrefab.transform.position = target;
                selectedTileGridCoords = tile;

                if (true) // Debug
                {
                    DebugPrintTileInfo();
                }

                // If we've selected an enemy, show its canvas
                if (BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].tileEntityType == Tile.TileEntityType.enemy)
                {
                    ((EnemyBody)BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].GetEntityOnTile()).EnemyUI.DisplayHealthBar();
                }
            }

        }
    }

    private void DebugPrintTileInfo()
    {
        Debug.Log("Selected tile entity: " + BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].tileEntityType.ToString());
        Debug.Log("Selected terrain: " + BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].tileTerrainType.ToString());

        TileItem item = BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].GetItemOnTile();
        if (item != null)
        {
            if (item is DroppedMoney)
                Debug.Log("Selected item: Money, value of " + (item as DroppedMoney).Value);
            else
                Debug.Log("Selected item: Unknown. Please update this in PlayerUIManager.");
        }
        else
        {
            Debug.Log("Selected item: None.");
        }
    }

    // Destroys all the object prefabs that are used to highlight tiles for card ranges. Then clears out the list.
    private void DestroyHighlightTiles()
    {
        while (spawnedTileHighlightPrefabs.Count > 0)
        {
            Destroy(spawnedTileHighlightPrefabs[0]);
            spawnedTileHighlightPrefabs.RemoveAt(0);
        }
    }

    private void SpawnHighlightTiles(bool[,] highlightArray)
    {
        DestroyHighlightTiles(); // Clear out previous highlighted tiles
        int sizeX = highlightArray.GetLength(0);
        int sizeY = highlightArray.GetLength(1);

        Transform ground = BattleGrid.instance.transform;

        int gridPlayerLOCX = (int)(sizeX / 2 - 1);
        int gridPlayerLOCY = (int)(sizeY / 2 - 1);

        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeY; j++)
            {
                // If the corresponding slot is true, spawn a highlighting tile there relative to the player.
                if (highlightArray[i, j])
                {
                    spawnedTileHighlightPrefabs.Add(Instantiate(tileHighlightPrefab, new Vector3(pc.xPos + i - gridPlayerLOCX - 1f, 0.2f, pc.zPos + j - gridPlayerLOCY - 1f), Quaternion.identity, ground));
                }
            }
        }

    }
    #endregion

    #region CardPlayingAndRanging

    // Generates the range array for the selected card
    private bool[,] GenerateCardRange(Card cardData)
    {
        var cardRange = cardData.Range;
        bool needsLOS = false;
        bool needsEmptyTile = false;
        bool cornerCutting = false;
        foreach (var x in cardRange.PlayConditions)
        {
            switch (x)
            {
                case PlayCondition.needsLOS:
                    needsLOS = true;
                    break;
                case PlayCondition.emptyTile:
                    needsEmptyTile = true;
                    break;
                case PlayCondition.cornerCutting:
                    cornerCutting = true;
                    break;
            }
        }

        bool[,] range = cardRange.RangeArray;

        if (needsLOS)
        {
            if (cornerCutting)
                range = BattleManager.ANDArray(pc.LoSGrid, range);
            else
                range = BattleManager.ANDArray(pc.SimpleLoSGrid, range);
        }

        if (needsEmptyTile)
            range = ClearNonEmptyTiles(range);

        return range;
    }

    // For each 1, switch it to 0 if that tile is not empty.
    private bool[,] ClearNonEmptyTiles(bool[,] range)
    {
        int sizeX = range.GetLength(0);
        int sizeZ = range.GetLength(1);
        int gridPlayerLOCX = (int)(sizeX / 2 - 1);
        int gridPlayerLOCY = (int)(sizeZ / 2 - 1);
        for (int i = 0; i < sizeX; i++)
        {
            for (int j = 0; j < sizeZ; j++)
            {
                if (range[i, j])
                {
                    // Remove if not empty
                    if (BattleManager.instance.map.map[pc.xPos + i - gridPlayerLOCX - 1, pc.zPos + j - gridPlayerLOCY - 1].tileEntityType != Roguelike.Tile.TileEntityType.empty)
                    {
                        range[i, j] = false;
                    }
                }

            }
        }

        return range;
    }

    // Attempts to play the card on the given tile
    private void CheckTilePlayConditions(CardInterface cardToPlay, Vector2Int target)
    {
        // Is this tile an acceptable target?
        BattleGrid.AcceptableTileTargetFailReasons failReason = BattleGrid.instance.AcceptableTileTarget(target, cardToPlay.cardData);

        if (failReason == BattleGrid.AcceptableTileTargetFailReasons.mustTargetCreature)
        {
            // Needed to hit a creature, and didn't.
            ShowAlert("Damage dealing cards must target creatures");
        }
        else
        {
            // Can hit this square. Inform the pc.
            PlayerController.AttemptToPlayCardFailReasons reason = pc.AttemptToPlayCard(cardToPlay, target);
            switch (reason)
            {
                case PlayerController.AttemptToPlayCardFailReasons.notEnoughEnergy:
                    ShowAlert("Not enough energy for card.");
                    break;
                case PlayerController.AttemptToPlayCardFailReasons.notPlayerTurn:
                    ShowAlert("It's not currently your turn");
                    break;
                case PlayerController.AttemptToPlayCardFailReasons.bossInvincible:
                    ShowAlert("This enemy is currently invulnerable");
                    break;
                case PlayerController.AttemptToPlayCardFailReasons.success:
                    // Discarding is handled by ResolveCard() in the pc.
                    AudioManager.PlayCardSound(cardToPlay.cardData.CardInfo.Name);
                    DestroyHighlightTiles();
                    if (cardToPlay == selectedCard)
                        DeselectCard();
                    break;

            }
        }
    }

    #endregion

    // Handles when the player has ended their turn.
    internal void EndOfTurn()
    {
        DeselectCard();
        DeselectTile();
    }

    #region UISetting
    internal void SetTurnsUntilDrawText(int amount)
    {
        //turnsUntilDrawTextComponent.SetText(amount + "");
        //Debug.Log("This used to be to set TurnsUntilDrawText... but I removed it.");
    }

    // Fills in the circle to the percentage given.
    public void SetSpiritPercentage(float amount)
    {
        spiritFillImage.fillAmount = amount;
        spiritTextComponent.SetText((amount * 100).ToString("0") + "%");
    }

    // Sets the text for the circle showing current energy
    public void SetCurrentEnergy(int amount)
    {
        energyTextComponent.SetText(amount.ToString());
    }

    public void SetCurrentHealth(int amount, int maxHealth)
    {
        healthFillImage.fillAmount = Mathf.Clamp((1.0f * amount) / maxHealth, 0, 1f);
        healthTextComponent.SetText(amount + "");
    }

    public void SetCurrentMoney(int amount)
    {
        moneyDisplayLabel.SetText("Coins: " + amount);
    }

    #endregion

    #region UIManipulation

    bool cardDrawerAtTop = true;
    /// <summary>
    /// Toggles the card drawer's position and makes it go either to the top or bottom.
    /// </summary>
    public void ToggleCardDrawerPosition()
    {
        GameObject hand = handLayout.transform.parent.gameObject;
        GameObject arrowImage = GameObject.Find("MoveCardViewButtonImage");
        GameObject arrowButton = arrowImage.transform.parent.gameObject;
        arrowButton.GetComponent<Button>().enabled = false;
        //float MoveAmount = 420; //420
        if (cardDrawerAtTop)
        {
            iTween.MoveTo(hand, iTween.Hash("y", -240, "easeType", "easeInOutSine", "loopType", "none", "delay", 0.0f, "isLocal", true));
            //iTween.MoveTo(arrowButton, iTween.Hash("y", -200, "easeType", "easeInOutSine", "loopType", "none", "delay", 0.05f, "isLocal", true));
            iTween.RotateTo(arrowImage, iTween.Hash("rotation", new Vector3(0, 0, 0), "easeType", "easeInOutSine", "loopType", "none", "time", 1.2f, "isLocal", true,
                "onComplete", "ReenableMoveCardDrawerButton", "onCompleteTarget", gameObject));
            // 5.25 / 1.35 = 3.88888888889
        }
        else
        {
            iTween.MoveTo(hand, iTween.Hash("y", 542, "easeType", "easeInOutSine", "loopType", "none", "delay", 0.0f, "isLocal", true));
            //iTween.MoveTo(arrowButton, iTween.Hash("y", 0, "easeType", "easeInOutSine", "loopType", "none", "delay", 0.05f, "isLocal", true));
            iTween.RotateTo(arrowImage, iTween.Hash("rotation", new Vector3(0, 0, 180), "easeType", "easeInOutSine", "loopType", "none", "time", 1.2f, "isLocal", true,
                "onComplete", "ReenableMoveCardDrawerButton", "onCompleteTarget", gameObject));
        }

        cardDrawerAtTop = !cardDrawerAtTop;
    }

    /// <summary>
    /// ToggleCardDrawerPosition() disables the button. This is called by iTween and reenables it.
    /// </summary>
    public void ReenableMoveCardDrawerButton()
    {
        GameObject arrowImage = GameObject.Find("MoveCardViewButtonImage");
        GameObject arrowButton = arrowImage.transform.parent.gameObject;
        arrowButton.GetComponent<Button>().enabled = true;
    }

    public void ShowAlert(string text)
    {
        notificationBarFade.StartFadeCycle(text, 0.75f, 0.5f);
    }

    private void UpdateHandLayoutSpacing()
    {
        if (graphicalHand.Count > 9)
        {
            int spacingAmount = -75;
            spacingAmount = Math.Max(spacingAmount, (-graphicalHand.Count + 9) * 12);
            handLayout.spacing = spacingAmount;
        }
        else
        {
            handLayout.spacing = 5;
        }

    }
    #endregion

    #region Status Effects
    // Destroys the icon for a status effect 
    public void RemoveStatusEffect(StatusEffectDataHolder baseObj)
    {
        Debug.Log("Destroying icon " + baseObj.iconInterface.gameObject.name);
        Destroy(baseObj.iconInterface.gameObject);
    }

    // Creates a dataholder and spawns a new icon.
    public StatusEffectDataHolder AddNewStatusEffect(BattleManager.StatusEffectEnum setType, int val)
    {
        Debug.Log("Spawning new status effect.");
        StatusEffectDataHolder baseObj = new StatusEffectDataHolder();
        GameObject go = Instantiate(statusEffectIconPrefab, statusEffectLayout.transform, false);
        go.name = "StatusIconFor" + setType.ToString();
        Debug.Log("spawned " + go.name);
        baseObj.iconInterface = go.GetComponent<StatusEffectIconInterface>();
        baseObj.iconInterface.SetStatusEffect(setType);
        baseObj.EffectValue = val; // Setter method will update the number
        return baseObj;
    }
    #endregion

    #region playerUIStateMachine

    // Changes our state to the given one.
    // Calls 'ExitState' on the state we leave, and 'EnterState' on the state we enter.
    public bool MoveToState(PlayerUIState state)
    {
        Debug.Log("Attempting to move from " + playerUIState.internalUIState.ToString() + " to " + state.ToString());
        if (CanMoveToState(state))
        {
            Debug.Log("Moving states");
            ExitState();
            EnterState(state);
            return true;
        }
        else
            return false;
    }

    // Checks if we can move from the current state to 'state'.
    private bool CanMoveToState(PlayerUIState state)
    {
        switch (state)
        {
            case PlayerUIState.controllingCamera:
                switch (playerUIState.internalUIState)
                {
                    case PlayerUIState.massCardView: // Cannot move from masscardview to controlling camera.
                    case PlayerUIState.inEventChoice:
                        return false;
                }
                break;
        }

        return true;
    }

    // Does whatever must be done to leave a state.
    private void ExitState()
    {
        switch (playerUIState.internalUIState)
        {
            case PlayerUIState.standardCardDrawer:
                DeselectCard();
                break;
            case PlayerUIState.controllingCamera:
                vcam.Follow = transform; // Lock camera
                vcam.LookAt = transform;
                break;
            case PlayerUIState.massCardView:
                massCardViewBackground.gameObject.SetActive(false);
                backShading.SetActive(false);
                break;
            case PlayerUIState.cardRewardView:
                cardRewardViewbackground.gameObject.SetActive(false);
                backShading.SetActive(false);
                break;
            case PlayerUIState.inEventChoice:
                pickChoiceView.gameObject.SetActive(false);
                backShading.SetActive(false);
                break;
        }
    }

    private void EnterState(PlayerUIState state)
    {
        switch (state)
        {
            case PlayerUIState.controllingCamera:
                vcam.Follow = null; // Unlock camera
                vcam.LookAt = null;
                ShowAlert("Press P again to disable camera panning.");
                break;
            case PlayerUIState.massCardView: // Open mass card view
                massCardViewBackground.gameObject.SetActive(true);
                backShading.SetActive(true);
                PopulateMassCardView();
                break;
            case PlayerUIState.cardRewardView: // Open card reward view
                cardRewardViewbackground.gameObject.SetActive(true);
                backShading.SetActive(true);
                PopulateCardRewardView();
                break;
            case PlayerUIState.inEventChoice: // Open event choice view
                pickChoiceView.gameObject.SetActive(true);
                backShading.SetActive(true);
                PopulateEventChoiceView();
                break;
        }
        playerUIState.internalUIState = state;
    }

    public PlayerUIState GetState()
    {
        return playerUIState.internalUIState;
    }

    public bool IsStateMovementLocked()
    {
        if (playerUIState.internalUIState == PlayerUIState.standardCardDrawer && !playerUIState.tooltipOpen)
        {
            return false;
        }
        else
            return true;
    }

    public bool IsStateControllingCamera()
    {
        if (playerUIState.internalUIState == PlayerUIState.controllingCamera)
        {
            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// This is called when the backshading is clicked.
    /// </summary>
    public void CloseOutOfWindows()
    {
        Debug.Log("Backshading clicked.");
        if (playerUIState.tooltipOpen) // Closing the tooltip takes priority
        {
            CloseToolTip();
            return;
        }

        switch (playerUIState.internalUIState)
        {
            case PlayerUIState.cardRewardView:
                ShowAlert("Pick a card or pick skip.");
                return;
            case PlayerUIState.inEventChoice:
                ShowAlert("You must pick a choice.");
                break;
            default:
                MoveToState(PlayerUIState.standardCardDrawer);
                return;
        }
    }

    #endregion

    #region MassCardView
    // Variables that indicate which sets of cards to include in the mass card view.
    // Order is Draw, Discard, Hand, Banish.
    // Need to shuffle draw.
    private bool[] whichCardsToInclude = { false, false, false, false };
    public void ToggleMassCardViewOption(int tar)
    {
        whichCardsToInclude[tar] = !whichCardsToInclude[tar];
        PopulateMassCardView();
    }

    //{ standardCardDrawer, standardNoCardDrawer, controllingCamera, massCardView}
    // Can only open masscardview if we're in standard. Close if we're in massCardView.
    public void ToggleShowMassCardView()
    {
        PlayerUIState state = GetState();
        switch (state)
        {
            case PlayerUIState.standardCardDrawer: // Open window
                MoveToState(PlayerUIState.massCardView);
                break;
            case PlayerUIState.massCardView: // Close window
                MoveToState(PlayerUIState.standardCardDrawer);
                break;
            default:
                break;
        }
    }

    private void PopulateMassCardView() // Clears out the mass card view, then spawns cards into it.
    {

        Debug.Log("Populating. Hand: " + whichCardsToInclude[0] + ", discard: " + whichCardsToInclude[1] + ", hand: " + whichCardsToInclude[2]);
        // Clear it out
        foreach (Transform t in massCardViewGridLayout.transform)
        {
            BattleManager.RecursivelyEliminateObject(t);
        }

        // Spawn cards in deck if able
        if (whichCardsToInclude[0])
        {
            List<Card> shuffledDeck = new List<Card>(BattleManager.player.playerDeck.drawPile.Count); // Need to make a temp list to shuffle them
            foreach (Card c in BattleManager.player.playerDeck.drawPile)
            {
                shuffledDeck.Insert(UnityEngine.Random.Range(0, shuffledDeck.Count), c);
            }

            // Now add these cards to the view
            foreach (Card c in shuffledDeck)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, massCardViewGridLayout.transform);
                CardInterface ci = newCard.GetComponent<CardInterface>();
                ci.cardData = c;
                ci.OnCardSpawned(CardInterface.CardInterfaceLocations.cardView);
            }
        }

        if (whichCardsToInclude[1])
        {
            foreach (Card c in BattleManager.player.playerDeck.discardPile)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, massCardViewGridLayout.transform);
                CardInterface ci = newCard.GetComponent<CardInterface>();
                ci.cardData = c;
                ci.OnCardSpawned(CardInterface.CardInterfaceLocations.cardView);
            }
        }

        if (whichCardsToInclude[2])
        {
            foreach (Card c in BattleManager.player.playerDeck.hand)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, massCardViewGridLayout.transform);
                CardInterface ci = newCard.GetComponent<CardInterface>();
                ci.cardData = c;
                ci.OnCardSpawned(CardInterface.CardInterfaceLocations.cardView);
            }
        }

        if (whichCardsToInclude[3])
        {
            foreach ((Card c, int i) in BattleManager.player.playerDeck.banishPile)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, massCardViewGridLayout.transform);
                // Now we need to stick a banish counter on it.
                GameObject bc = GameObject.Instantiate(banishCounter, newCard.transform);
                bc.GetComponent<TextMeshProUGUI>().SetText(i.ToString());
                CardInterface ci = newCard.GetComponent<CardInterface>();
                ci.cardData = c;
                ci.OnCardSpawned(CardInterface.CardInterfaceLocations.cardView);
            }
        }

    }
    #endregion

    #region Camera
    // This method takes movement input and applies it to the camera.
    public void HandleCameraMovement()
    {
        float xDir = (Input.GetAxis("Horizontal"));
        float yDir = Input.mouseScrollDelta.y * -20;
        float zDir = (Input.GetAxis("Vertical"));
        Vector3 movementVector = new Vector3(xDir, yDir, zDir) * Time.deltaTime * 10f;

        // Now snap it inside the bounds of the map
        Vector3 snappedPosition = vcam.transform.position + movementVector;

        // Top right of the map
        Vector3 maxPosition = BattleManager.ConvertVector(new Vector2Int(BattleGrid.instance.sizeX, BattleGrid.instance.sizeZ), 0);

        if (snappedPosition.x < 0)
            snappedPosition.x = 0;
        else if (snappedPosition.x > maxPosition.x)
            snappedPosition.x = maxPosition.x;

        if (snappedPosition.y < 5)
            snappedPosition.y = 5;
        else if (snappedPosition.y > 25)
            snappedPosition.y = 25;

        if (snappedPosition.z < 0)
            snappedPosition.z = 0;
        else if (snappedPosition.z > maxPosition.z)
            snappedPosition.z = maxPosition.z;

        vcam.transform.position = snappedPosition;
    }
    #endregion

    #region CardRewardScreen

    public void OpenCardRewardView()
    {
        // Move to card reward viewing state.
        MoveToState(PlayerUIState.cardRewardView);
    }

    // Spawns 3 cards for the player to pick from.
    // Why 3? All the cool games do 3.
    private void PopulateCardRewardView()
    {
        Debug.Log("Populating card reward screen.");
        // Clear it out
        foreach (Transform t in cardRewardHorizontalLayout.transform)
        {
            BattleManager.RecursivelyEliminateObject(t);
        }

        // Now spawn 3 random cards.
        for (int i = 0; i < 3; i++)
        {
            // For now, just pick randomly.
            Card pickedCard = CardFactory.GetRandomCard();
            GameObject newCard = GameObject.Instantiate(cardPrefab, cardRewardHorizontalLayout.transform);
            CardInterface ci = newCard.GetComponent<CardInterface>();
            ci.cardData = pickedCard;
            ci.OnCardSpawned(CardInterface.CardInterfaceLocations.cardReward);
        }
    }

    /// <summary>
    /// Leaves the CardRewardScreen
    /// </summary>
    /// <param name="pickedSomething">True if the player selected a card. False if they skipped.</param>
    public void LeaveCardRewardScreen(bool pickedSomething)
    {
        if (!pickedSomething)
            BattleManager.player.TakeDamage(BattleManager.ConvertVector(transform.position), -20); // Heal player for 10

        // Clear it out
        foreach (Transform t in cardRewardHorizontalLayout.transform)
        {
            BattleManager.RecursivelyEliminateObject(t);
        }

        // Exit out of state
        MoveToState(PlayerUIState.standardCardDrawer);
    }

    public bool CanGetCardReward()
    {
        if (playerUIState.internalUIState == PlayerUIState.inEventChoice)
        {
            ShowAlert("You must resolve this event first.");
            return false;
        }
        else
            return true;
    }

    #endregion

    #region CardTooltipScreen

    // Opens the tooltip window for one card.
    public void ToolTipRequest(Card cardinfo)
    {
        if (playerUIState.tooltipOpen == false)
        { // We need to open it.
            OpenToolTip();
        }

        ClearToolTip();
        PopulateToolTip(cardinfo);
    }

    private void OpenToolTip()
    {
        tooltipView.SetActive(true);
        playerUIState.tooltipOpen = true;
        backShading.SetActive(true);
    }

    /// <summary>
    /// Closes the tooltip. If the player is in a ui state that wouldn't normally have the backshading, also hides that.
    /// </summary>
    private void CloseToolTip()
    {
        tooltipView.SetActive(false);
        playerUIState.tooltipOpen = false;
        if (playerUIState.internalUIState == PlayerUIState.controllingCamera || playerUIState.internalUIState == PlayerUIState.standardCardDrawer)
        {
            backShading.SetActive(false);
        }
    }

    /// <summary>
    /// Clears out an open tooltip. Clears the card on display, as well as all text tool tips.
    /// </summary>
    private void ClearToolTip()
    {
        foreach (Transform t in tooltipCardHolder.transform)
        {
            BattleManager.RecursivelyEliminateObject(t);
        }
    }

    /// <summary>
    /// Fills the text boxes with details about the requested card.
    /// </summary>
    /// <param name="cardInfo"></param>
    private void PopulateToolTip(Card cardInfo)
    {
        GameObject newCard = GameObject.Instantiate(cardPrefab, tooltipCardHolder.transform);

        CardInterface ci = newCard.GetComponent<CardInterface>();
        ci.cardData = cardInfo;
        ci.OnCardSpawned(CardInterface.CardInterfaceLocations.tooltip);

        // Set size and location
        newCard.transform.localScale = new Vector3(3, 3, 1);
        newCard.transform.localPosition = new Vector3(-280, 0, 0);

        TextMeshProUGUI tooltipText = tooltipContent.GetComponentInChildren<TextMeshProUGUI>();
        string promptText = "";

        // Fill prompt box.
        if (cardInfo.Prompts != null)
        {
            foreach (Card.CardTooltipPrompts x in cardInfo.Prompts)
            {
                if (promptText != "")
                {
                    promptText += "\n\n";
                }

                switch (x)
                {
                    case Card.CardTooltipPrompts.line:
                        promptText += "<b>Line</b>: This card must target a space in a straight line in one of the eight cardinal directions.";
                        break;
                    case Card.CardTooltipPrompts.cutscorners:
                        promptText += "<b>Corner Cutting</b>: This card uses a more forgiving line of sight calculation, allowing you to target across corners.";
                        break;
                    case Card.CardTooltipPrompts.move:
                        promptText += "<b>Move</b>: This card will move you to the targeted tile. Does not end your turn, and must target an empty tile.";
                        break;
                    case Card.CardTooltipPrompts.defense:
                        promptText += "<b>Defense</b>: Defense is a status condition that takes damage before your health does. Decays at a rate of one per turn.";
                        break;
                    case Card.CardTooltipPrompts.momentum:
                        promptText += "<b>Momentum</b>: Momentum boosts the damage of your next played card. Works well with multi-hit cards. Decays while not in combat.";
                        break;
                    case Card.CardTooltipPrompts.insight:
                        promptText += "<b>Insight</b>: Each point of insight boosts your next <i>instance</i> of damage by 100%. Works well with big single-hit cards. Decays while not in combat.";
                        break;
                    case Card.CardTooltipPrompts.aoe:
                        promptText += "<b>AOE</b>: Means Area of Effect. Targets everything in a square of some radius. If it is centered on you, you don't get hit by it.";
                        break;
                    case Card.CardTooltipPrompts.lifesteal:
                        promptText += "<b>Lifesteal</b>: Any damage you deal with this card is gained as health.";
                        break;
                    case Card.CardTooltipPrompts.banish:
                        promptText += "<b>Banish</b>: Banish X means that after you play this card, it is unavailable for X floors, after which it is shuffled back into your discard pile.";
                        break;
                    case Card.CardTooltipPrompts.echo:
                        promptText += "<b>Echo</b>: This Echo effect will trigger if this card is discarded by another card's effect. Echo effects do not cost energy or spirit.";
                        break;
                }
            }
        }

        // Next, add a prompt based on the theme.
        if (promptText != "")
        {
            promptText += "\n\n";
        }

        switch (cardInfo.CardInfo.ThemeName)
        {
            case "Draconic":
                promptText += "This card is in the Draconic set, which focuses on defense and devastating heavy strikes.<br>Ancient and powerful, these creatures command respect and fear";
                break;
            case "Impundulu":
                promptText += "This card is in the Impundulu set, which focuses on momentum, multihit cards, and lifesteal.<br>These vampiric thunderbirds are devious and relentless beasts.";
                break;
            case "AlMiraj":
                promptText += "This card is in the Al-Mi'raj set, which focuses on discard and Echo effects.<br>Deceptively vicious, these horned beasts devourer all in their path.";
                break;
            case "Hanafuda":
                promptText += "This is a hanafuda card, a powerful card that goes into your inventory rather than your deck, but can only be used once.";
                break;
            default:
                promptText += "This card isn't part of any set. Maybe it's because the developers forgot to assign it one.";
                break;
        }

        // Add a prompt based on lore.
        if (cardInfo.CardInfo.FlavorText != "")
        {
            if (promptText != "")
            {
                promptText += "\n\n";
            }
            promptText += "<i>" + cardInfo.CardInfo.FlavorText + "</i>";
        }
        tooltipText.SetText(promptText);
    }

    #endregion

    #region CardInventory

    bool canMoveInventory = true;
    public void ToggleInventoryVisible()
    {
        if (playerUIState.inventoryOpen == false && (playerUIState.internalUIState == PlayerUIState.controllingCamera || playerUIState.internalUIState == PlayerUIState.inEventChoice))
            return;

        if (canMoveInventory)
        {
            if (playerUIState.inventoryOpen)
                CloseInventory();
            else
                OpenInventory();
            canMoveInventory = false;
        }

    }

    /// <summary>
    /// ToggleCardDrawerPosition() disables the button. This is called by iTween and reenables it.
    /// </summary>
    public void ReenableMoveInventoryAbility()
    {
        canMoveInventory = true;
    }

    private void OpenInventory()
    {
        //iTween.MoveBy(inventoryView, iTween.Hash("x", -158.898, "easeType", "easeInOutSine", "loopType", "none", "time", 0.6f, "onComplete", "ReenableMoveInventoryAbility", "onCompleteTarget", gameObject, "islocal", true));
        playerUIState.inventoryOpen = true;
        iTween.MoveTo(inventoryView, iTween.Hash("x", 959, "easeType", "easeInOutSine", "loopType", "none", "time", 0.6f, "onComplete", "ReenableMoveInventoryAbility", "onCompleteTarget", gameObject, "islocal", true));
    }

    /// <summary>
    /// Closes your inventory
    /// </summary>
    private void CloseInventory()
    {
        //iTween.MoveBy(inventoryView, iTween.Hash("x", 158.898, "easeType", "easeInOutSine", "loopType", "none", "time", 0.3f, "onComplete", "ReenableMoveInventoryAbility", "onCompleteTarget", gameObject, "islocal", true));
        playerUIState.inventoryOpen = false;
        iTween.MoveTo(inventoryView, iTween.Hash("x", 1275, "easeType", "easeInOutSine", "loopType", "none", "time", 0.6f, "onComplete", "ReenableMoveInventoryAbility", "onCompleteTarget", gameObject, "islocal", true));
    }

    // A card has been added to the inventory. If we have the inventory open, add this card to it.
    public void CardAddedToInventory(Card info)
    {
        GameObject newCard = GameObject.Instantiate(cardPrefab, inventoryContent.transform);

        CardInterface ci = newCard.GetComponent<CardInterface>();
        ci.AssignTextMeshVariables();
        ci.cardData = info;
        ci.cardIndex = graphicalInventory.Count;
        graphicalInventory.Add(ci);
        ci.OnCardSpawned(CardInterface.CardInterfaceLocations.inventory);

        /*GameObject newCard = GameObject.Instantiate(cardPrefab, inventoryContent.transform);
        CardInterface ci = newCard.GetComponent<CardInterface>();
        ci.cardData = info;
        ci.OnCardSpawned(CardInterface.CardInterfaceLocations.inventory);
        graphicalInventory.Add(ci);*/
    }

    public void CardInInventoryClicked(int index)
    {
        Debug.Log("Inventory card clicked: " + index + ", selectedCard = " + selectedCard);
        // We're only allowed to click cards in standard UI state.
        if (IsStateMovementLocked())
        {
            return;
        }

        // If player has clicked on highlighted card, unhighlight it
        if (selectedCard != null && index == selectedCard.cardIndex && selectedCard.GetLocation() == CardInterface.CardInterfaceLocations.inventory)
        {
            DeselectCard();
        }
        else
        {
            // Unhighlight all other cards, highlights this one.
            DeselectCard();
            SelectCard(index, CardInterface.CardInterfaceLocations.inventory);
        }
    }

    // Destroys the card in the inventory graphically. Also removes the card from the graphical list. Does not alter the deck internal listing.
    internal void DestroyInventoryCardAtIndex(int index)
    {
        if (selectedCard && selectedCard.cardIndex == index && selectedCard.GetLocation() == CardInterface.CardInterfaceLocations.inventory)
        {
            // If we're discarding the selected card, deselect it.
            DeselectCard();
        }
        // Loop through and reduce the index of other cards by one
        for (int i = index + 1; i < graphicalInventory.Count; i++)
        {
            graphicalInventory[i].cardIndex--;
        }

        // Destroy the gameobject
        GameObject.Destroy(graphicalInventory[index].gameObject);

        // Now remove
        graphicalInventory.RemoveAt(index);
    }

    #endregion

    #region PickEventChoicePanel

    private EventDatabase.EventEnum currentEvent;
    public void RequestEvent(EventDatabase.EventEnum evn)
    {
        if (playerUIState.internalUIState != PlayerUIState.inEventChoice)
        {
            currentEvent = evn;
            MoveToState(PlayerUIState.inEventChoice);
        }
    }

    // This currently is only really set up for 3 choices. Later, we can have a more robust system that generates/destroys buttons.
    private void PopulateEventChoiceView()
    {
        // Get the three buttons.
        EventChoiceButton[] buttons = pickChoiceView.GetComponentsInChildren<EventChoiceButton>();
        List<int> pickedChoices = new List<int>();
        RogueEvent even = EventDatabase.GetEvent(currentEvent);

        for (int i = 0; i < buttons.Length; i++)
        {
            // Find a choice we haven't added yet to the dialog box.
            int pickedChoice;
            do
            {
                pickedChoice = Random.Range(0, even.NumChoices);
            } while (pickedChoices.Contains(pickedChoice));

            pickedChoices.Add(pickedChoice);
            buttons[i].SetValues(currentEvent, pickedChoice);
            buttons[i].buttonText.SetText(even.ChoiceDescriptions[pickedChoice]);
        }

        pickChoiceTextDescription.SetText(even.Description);
    }

    public void ResolveEvent(EventDatabase.EventEnum emu, int pickedChoice)
    {
        if (playerUIState.internalUIState != PlayerUIState.inEventChoice)
        {
            Debug.LogWarning("Warning, PlayerUIManager was asked to resolve an event when we're not in the correct ui state.");
            return;
        }
        MoveToState(PlayerUIState.standardCardDrawer);

        EventDatabase.ResolveEvent(emu, pickedChoice);

    }

    #endregion
}