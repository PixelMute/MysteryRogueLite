using Cinemachine;
using Roguelike;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    private Image cardRewardViewbackground;
    private HorizontalLayoutGroup cardRewardHorizontalLayout;

    // Ones that should be assigned in the editor.
    [SerializeField] private TextMeshProUGUI moneyDisplayLabel;
    [SerializeField] private GameObject BackShading;

    // Player UI FSM.
    // TODO: Maybe change this over to a bunch of objects, so we can call State.Exit() instead of Exit(state)
    public enum PlayerUIState { standardCardDrawer, controllingCamera, massCardView, cardRewardView }
    private PlayerUIState playerUIState = PlayerUIState.standardCardDrawer;

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
    [HideInInspector] public Vector3 selectedTile;

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
                                            "DeckViewContent", "View - MassCardView", "View - GainCardView", "CardOptionsHolder", "HealthBarFill"};
        Type[] typeNames = new Type[] {typeof(TextMeshProUGUI), typeof(TextMeshProUGUI), typeof(TextMeshProUGUI), typeof(Image), typeof(TextMeshProUGUI), typeof(HorizontalLayoutGroup),
        typeof (GridLayoutGroup), typeof (GridLayoutGroup), typeof(Image), typeof(Image), typeof(HorizontalLayoutGroup), typeof(Image)};
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
        cardRewardViewbackground = valArray[9] as Image;
        cardRewardHorizontalLayout = valArray[10] as HorizontalLayoutGroup;
        healthFillImage = valArray[11] as Image;

        graphicalHand = new List<CardInterface>();
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
        handLayout.SetLayoutHorizontal();
        GameObject newCard = GameObject.Instantiate(cardPrefab, handLayout.transform);

        CardInterface ci = newCard.GetComponent<CardInterface>();
        ci.cardData = drawnCard;
        ci.cardHandIndex = graphicalHand.Count;
        graphicalHand.Add(ci);
        ci.OnCardSpawned(CardInterface.CardInterfaceLocations.hand);

        selectedCard = null;
    }

    // Note that this destroys the card graphically. It does not alter anything about the data.
    internal void DestroyCardAtIndex(int index)
    {
        if (selectedCard && selectedCard.cardHandIndex == index)
        {
            // If we're discarding the selected card, deselect it.
            DeselectCard();
        }
        // Loop through and reduce the index of other cards by one
        for (int i = index + 1; i < graphicalHand.Count; i++)
        {
            Debug.Log("Reducing index of number " + i);
            graphicalHand[i].cardHandIndex--;
        }

        // Destroy the gameobject
        GameObject.Destroy(graphicalHand[index].gameObject);

        // Now remove
        graphicalHand.RemoveAt(index);
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
        if (selectedCard && Time.time - initialClickTime < doubleClickTime && selectedCard.cardHandIndex == index)
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
        if (selectedCard != null && index == selectedCard.cardHandIndex)
        {
            DeselectCard();
        }
        else
        {
            // Unhighlight all other cards, highlights this one.
            DeselectCard();
            SelectCard(index);
        }
    }

    internal void SelectCard(int index)
    {
        graphicalHand[index].Highlight();
        selectedCard = graphicalHand[index];

        // Now we need to highlight all the squares where this can be played.

        selectedCardRange = GenerateCardRange(graphicalHand[index].cardData);
        SpawnHighlightTiles(selectedCardRange);
    }

    // Deselects whatever card we have selected
    private void DeselectCard()
    { 
        if (selectedCard != null)
        {
            if (selectedCard.cardHandIndex < graphicalHand.Count)
            {
                selectedCard.DisableHighlight();
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
            ((GenericEnemy)BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].GetEntityOnTile()).HideHealthBar(0.3f, 1f);
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
            Debug.Log("Clicked World. Selecting Tile.");
            // Check to make sure player has clicked within selectable area.
            if (BattleManager.IsVectorNonNeg(tile) && tile.x < BattleGrid.instance.map.GetLength(0) && tile.y < BattleGrid.instance.map.GetLength(1))
            {
                DeselectTile();
                tileCurrentlySelected = true;
                spawnedTileSelectionPrefab.SetActive(true);
                spawnedTileSelectionPrefab.transform.position = target;
                selectedTileGridCoords = tile;
                selectedTile = target;

                // If we've selected an enemy, show its canvas
                if (BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].tileEntityType == Tile.TileEntityType.enemy)
                {
                    Debug.Log("Enemy Selected");
                    ((GenericEnemy)BattleGrid.instance.map[selectedTileGridCoords.x, selectedTileGridCoords.y].GetEntityOnTile()).DisplayHealthBar();
                }
            }

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
                case PlayerController.AttemptToPlayCardFailReasons.success:
                    // Discarding is handled by ResolveCard() in the pc.
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
        if (cardDrawerAtTop)
        {
            iTween.MoveBy(hand, iTween.Hash("y", -5.25, "easeType", "easeInOutSine", "loopType", "none", "delay", 0.0f));
            iTween.MoveBy(arrowButton, iTween.Hash("y", 1.35, "easeType", "easeInOutSine", "loopType", "none", "delay", 0.05f));
            iTween.RotateTo(arrowImage, iTween.Hash("rotation", new Vector3(0, 0, 0), "easeType", "easeInOutSine", "loopType", "none", "time", 1.2f, "isLocal", true,
                "onComplete", "ReenableMoveCardDrawerButton", "onCompleteTarget", gameObject));
        }
        else
        {
            iTween.MoveBy(hand, iTween.Hash("y", 5.25, "easeType", "easeInOutSine", "loopType", "none", "delay", 0.0f));
            iTween.MoveBy(arrowButton, iTween.Hash("y", -1.35, "easeType", "easeInOutSine", "loopType", "none", "delay", 0.05f));
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
        notificationBarFade.StartFadeCycle(text, 0.75f, 1f);
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
        Debug.Log("Attempting to move from " + playerUIState.ToString() + " to " + state.ToString());
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
                switch (playerUIState)
                {
                    case PlayerUIState.massCardView: // Cannot move from masscardview to controlling camera.
                        return false;
                }
                break;
        }

        return true;
    }

    // Does whatever must be done to leave a state.
    private void ExitState()
    {
        switch (playerUIState)
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
                BackShading.SetActive(false);
                break;
            case PlayerUIState.cardRewardView:
                cardRewardViewbackground.gameObject.SetActive(false);
                BackShading.SetActive(false);
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
                BackShading.SetActive(true);
                PopulateMassCardView();
                break;
            case PlayerUIState.cardRewardView: // Open card reward view
                cardRewardViewbackground.gameObject.SetActive(true);
                BackShading.SetActive(true);
                PopulateCardRewardView();
                break;
        }
        playerUIState = state;
    }

    public PlayerUIState GetState()
    {
        return playerUIState;
    }

    public bool IsStateMovementLocked()
    {
        if (playerUIState == PlayerUIState.standardCardDrawer)
        {
            return false;
        }
        else
            return true;
    }

    public bool IsStateControllingCamera()
    {
        if (playerUIState == PlayerUIState.controllingCamera)
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
        if (playerUIState == PlayerUIState.cardRewardView)
        {
            ShowAlert("Pick a card or pick skip.");
            return;
        }
        else
        {
            MoveToState(PlayerUIState.standardCardDrawer);
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
            List<Card> shuffledDeck = new List<Card>(PlayerController.playerDeck.drawPile.Count); // Need to make a temp list to shuffle them
            foreach (Card c in PlayerController.playerDeck.drawPile)
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
            foreach (Card c in PlayerController.playerDeck.discardPile)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, massCardViewGridLayout.transform);
                CardInterface ci = newCard.GetComponent<CardInterface>();
                ci.cardData = c;
                ci.OnCardSpawned(CardInterface.CardInterfaceLocations.cardView);
            }
        }

        if (whichCardsToInclude[2])
        {
            foreach (Card c in PlayerController.playerDeck.hand)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, massCardViewGridLayout.transform);
                CardInterface ci = newCard.GetComponent<CardInterface>();
                ci.cardData = c;
                ci.OnCardSpawned(CardInterface.CardInterfaceLocations.cardView);
            }
        }

        if (whichCardsToInclude[3])
        {
            foreach ((Card c, int i) in PlayerController.playerDeck.banishPile)
            {
                GameObject newCard = GameObject.Instantiate(cardPrefab, massCardViewGridLayout.transform);
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
            // For now, just pick a random number from 1-23
            int pickedCard = UnityEngine.Random.Range(1, 24);
            GameObject newCard = GameObject.Instantiate(cardPrefab, cardRewardHorizontalLayout.transform);
            CardInterface ci = newCard.GetComponent<CardInterface>();
            ci.cardData = CardFactory.GetCardByID(pickedCard);
            Debug.Log("Picked card number " + pickedCard);
            Debug.Log("Picked number " + pickedCard + ", which is card " + ci.cardData.CardInfo.Name);
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
            BattleManager.player.TakeDamage(-20); // Heal player for 10

        // Clear it out
        foreach (Transform t in cardRewardHorizontalLayout.transform)
        {
            BattleManager.RecursivelyEliminateObject(t);
        }

        // Exit out of state
        MoveToState(PlayerUIState.standardCardDrawer);
    }

    #endregion
}