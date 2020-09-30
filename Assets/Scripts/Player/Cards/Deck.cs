using System.Collections.Generic;
using UnityEngine;

// This is the data class that stores information regarding the deck of cards.
public class Deck
{
    public List<Card> drawPile;
    public List<Card> discardPile;
    public List<Card> hand;

    public static Deck instance; // Singleton

    public Deck()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            throw new System.Exception("Deck--Deck():: trying to make a second deck when one already exists.");
        }

        drawPile = new List<Card>();
        discardPile = new List<Card>();
        hand = new List<Card>();
    }

    public void InsertCardAtEndOfDrawPile(Card cardToInsert)
    {
        drawPile.Add(cardToInsert);
    }

    // Draws the top card, puts it into the hand, and returns it.
    // Recycles discard if needed. Returns null if both draw and discard are empty.
    public Card DrawCard()
    {
        if (drawPile.Count == 0)
        {
            // No card to draw. Can we recycle the discard?
            if (discardPile.Count > 0)
            {
                RecycleDiscard();
            }
            else
            {
                // No cards in draw pile, none in discard. Cannot draw.
                return null;
            }
        }
        Card drawnCard = drawPile[0];
        drawPile.RemoveAt(0);
        hand.Add(drawnCard);
        return drawnCard;
    }

    // Clears out the discard pile and sticks it into the draw pile. Then it calls ShuffleDeck.
    public void RecycleDiscard()
    {
        drawPile.AddRange(discardPile);
        discardPile.Clear();
        ShuffleDeck();
    }

    // Shuffles the deck to make it random
    public void ShuffleDeck()
    {
        Debug.Log("Deck--ShuffleDeck()::Every day I'm shufflin");
        int n = drawPile.Count;
        while (n > 1)
        {
            n--;
            int k = UnityEngine.Random.Range(0, n);
            // Swap
            Card swapped = drawPile[k];
            drawPile[k] = drawPile[n];
            drawPile[n] = swapped;
        }
    }

    // Discards card[index] in the hand
    public void DiscardCardAtIndex(int index)
    {
        Card discarded = hand[index];
        hand.RemoveAt(index);
        discardPile.Add(discarded);
    }

    // Discards the players entire hand
    public void DiscardHand()
    {
        discardPile.AddRange(hand);
        hand.Clear();
    }

    public int DrawPileCount()
    {
        return drawPile.Count;
    }

}
