using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

// This class inherits from MonoBehavior to interface between the card data and Unity
public class CardInterface : MonoBehaviour, IPointerClickHandler
{
    public Card cardData;

    [SerializeField] private static GameObject highlightParticlePrefab;

    private TextMeshProUGUI cardTitleTextfield;
    private TextMeshProUGUI cardDescTextfield;
    private TextMeshProUGUI energyCostTextField;
    private TextMeshProUGUI spiritCostTextField;
    private ParticleFollowPath[] selectionParticleSystems;

    [HideInInspector] public int cardIndex; // Index of which card this is, in either inventory or hand.

    [HideInInspector] public bool isHighlighted = false;

    // Indicates where on the ui this card is.
    public enum CardInterfaceLocations { hand, cardView, cardReward, tooltip, inventory };
    private CardInterfaceLocations location;

    private void Awake()
    {
        if (cardTitleTextfield == null)
        {
            AssignTextMeshVariables();
        }
    }

    // Finds all textmesh objects on this and assigns the variables to them
    public void AssignTextMeshVariables()
    {
        string[] objNames = new string[] { "CardEnergyCostTextObject", "CardSpiritCostTextObject", "CardTitleTextObject", "CardDescriptionTextObject", "HighlightParticleSystemRight", "HighlightParticleSystemLeft" };
        Type[] typeNames = new Type[] { typeof(TextMeshProUGUI), typeof(TextMeshProUGUI), typeof(TextMeshProUGUI), typeof(TextMeshProUGUI), typeof(ParticleFollowPath), typeof(ParticleFollowPath) };
        Component[] partArray = BattleManager.RecursiveVariableAssign(gameObject, objNames, typeNames);

        cardTitleTextfield = partArray[2] as TextMeshProUGUI;
        cardDescTextfield = partArray[3] as TextMeshProUGUI;
        energyCostTextField = partArray[0] as TextMeshProUGUI;
        spiritCostTextField = partArray[1] as TextMeshProUGUI;

        selectionParticleSystems = new ParticleFollowPath[2];
        selectionParticleSystems[0] = partArray[4] as ParticleFollowPath;
        selectionParticleSystems[1] = partArray[5] as ParticleFollowPath;
    }

    // Highlights this card
    public void Highlight()
    {
        Debug.Log("Am I highlighted? " + isHighlighted);
        if (isHighlighted)
            return;
        else
        {
            // Enable the two particle systems.
            Debug.Log("Starting particles");
            selectionParticleSystems[0].StartMovement();
            selectionParticleSystems[1].StartMovement();
            isHighlighted = true;
        }
    }

    // Unhighlights this card.
    public void DisableHighlight()
    {
        if (!isHighlighted)
            return;
        else
        {
            selectionParticleSystems[0].DisableParticlePath();
            selectionParticleSystems[1].DisableParticlePath();
            isHighlighted = false;
        }

    }

    // This is called by the ui system when the card is clicked.
    public void RegisterClick()
    {
        switch (location)
        {
            case CardInterfaceLocations.hand:
                BattleManager.player.puim.CardInHandClicked(cardIndex);
                break;
            case CardInterfaceLocations.cardReward:
                BattleManager.player.TriggerCardReward(cardData);
                break;
            case CardInterfaceLocations.inventory:
                BattleManager.player.puim.CardInInventoryClicked(cardIndex);
                break;
            default:
                return;
        }

    }

    // Handles setting the details of the card when it is spawned.
    public void OnCardSpawned(CardInterfaceLocations location)
    {
        this.location = location;
        var info = cardData.CardInfo;
        cardTitleTextfield.SetText(info.Name);
        cardDescTextfield.SetText(info.Description);
        energyCostTextField.SetText(info.EnergyCost + "");
        spiritCostTextField.SetText(info.SpiritCost + "");
    }

    public CardInterfaceLocations GetLocation()
    {
        return location;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            RegisterClick();
        else if (eventData.button == PointerEventData.InputButton.Right && location != CardInterfaceLocations.tooltip)
        {
            BattleManager.player.puim.ToolTipRequest(cardData);
        }
    }
}
