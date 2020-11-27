// Tracks the results of a card stack.
// Used by cards to determine what happened as a result of them being played.

using Roguelike;
using UnityEngine;

public class CardStackTracker
{
    private bool active; // Are we currently tracking stuff?
    private int damageDealt = 0; // How much damage has been dealt over this tracking session
    private CardInterface currentlyResolvingCard; // If needed, we can make this an actual stack.
    private int usedMomentum = 0; // the amount of momentium this card stack has taken.

    public CardInterface CurrentlyResolvingCard { get => currentlyResolvingCard; private set => currentlyResolvingCard = value; }

    public void AddCardToTracker(CardInterface input)
    {
        CurrentlyResolvingCard = input;
        active = true;
    }

    // Resets how much we've tracked over the course of this session
    public void ResetTracker()
    {
        if (!active)
            return;

        active = false;
        damageDealt = 0;
        CurrentlyResolvingCard = null;
        usedMomentum = 0;
    }

    // Have we dealt at least this much?
    public bool QueryDamageDealt(int input)
    {
        return (damageDealt >= input);
    }

    public void AddDamageDealt(int input)
    {
        if (active)
            damageDealt += input;
    }

    public bool IsActive()
    {
        return active;
    }

    public int GetMomentumBonus()
    {
        int momun = BattleManager.player.GetMomentumBonus();
        usedMomentum = UnityEngine.Mathf.Max(usedMomentum, momun);
        return momun;
    }

    public float GetInsightBonus()
    {
        return BattleManager.player.GetInsightBonus();
    }

    public void SetUsedMomentum(int mo)
    {
        usedMomentum = mo;
    }

    public int QueryUsedMomentum()
    {
        return usedMomentum;
    }

    public CardInterface GetCurrentlyResolvingCard()
    {
        return CurrentlyResolvingCard;
    }

    /// <summary>
    /// Makes a request to discard or banish a card relative to the currently resolving card.
    /// </summary>
    /// <param name="target">Where the targeted card is. Either left, right, leftmost, rightmost.
    /// Card to the left/right fails if played card is the leftmost/rightmost.
    /// leftmost/rightmost fails if the played card is the leftmost/rightmost.</param>
    /// <param name="effect">Wether to discard or banish the card</param>
    /// <param name="value">Only applies to banish.</param>
    /// <returns>True if successful. False if failed.</returns>
    public bool MakeHandManipulateRequest(ManipulateHand.ManipulateHandTargetEnum target, ManipulateHand.ManipulateHandEffectEnum effect, int value)
    {
        int targetedCardID = -1;
        Debug.Log("Making a hand manipulate request. Input: " + target.ToString() + ", " + effect.ToString() + ", " + value);
        int currentlyResolvingCardId;
        if (currentlyResolvingCard == null)
        {
            if (target == ManipulateHand.ManipulateHandTargetEnum.leftCard || target == ManipulateHand.ManipulateHandTargetEnum.rightCard)
                throw new System.Exception("CardStackTracker::MakeHandManipulateRequest(" + target.ToString() + ", " + effect.ToString() + ", " + value + ")--There is no currently resolving card, but want a relative card position");

            currentlyResolvingCardId = -1;
        }
        else
        {
            currentlyResolvingCardId = currentlyResolvingCard.cardHandIndex;
        }
            

        switch (target)
        {
            case ManipulateHand.ManipulateHandTargetEnum.leftCard:
                targetedCardID = currentlyResolvingCardId - 1;
                break;
            case ManipulateHand.ManipulateHandTargetEnum.rightCard:
                targetedCardID = currentlyResolvingCardId + 1;
                break;
            case ManipulateHand.ManipulateHandTargetEnum.leftMostCard:
                targetedCardID = 0;
                break;
            case ManipulateHand.ManipulateHandTargetEnum.rightMostCard:
                targetedCardID = PlayerController.playerDeck.hand.Count - 1;
                break;
            case ManipulateHand.ManipulateHandTargetEnum.random:
                if (PlayerController.playerDeck.hand.Count == 0 || (currentlyResolvingCardId > -1 && PlayerController.playerDeck.hand.Count == 1))
                    return false; // Cannot select a random card.
                do
                {
                    targetedCardID = Random.Range(0, PlayerController.playerDeck.hand.Count - 1);
                } while (targetedCardID == currentlyResolvingCardId);
                break;
        }

        Debug.Log("Current Card Hand ID:" + targetedCardID + ", picking card " + targetedCardID);

        if (targetedCardID == currentlyResolvingCard.cardHandIndex || targetedCardID < 0 || targetedCardID >= PlayerController.playerDeck.hand.Count)
        { // Cannot target this card
            return false;
        }

        // Otherwise, we've targeted a valid card.
        if (effect == ManipulateHand.ManipulateHandEffectEnum.discard)
            BattleManager.player.DiscardCardIndex(targetedCardID, true);
        else
            BattleManager.player.BanishCardIndex(targetedCardID, value);

        return true;
    }
}
