using System.Collections.Generic;

public class DeckData
{
    List<int> drawPile = new List<int>();
    List<int> discardPile = new List<int>();
    List<int> hand = new List<int>();
    List<(int, int)> banishPile = new List<(int, int)>();
    List<int> inventoryCards = new List<int>();

    public static DeckData SaveDeck(Deck deck)
    {
        var deckData = new DeckData();
        foreach (var card in deck.drawPile)
        {
            deckData.drawPile.Add(card.CardInfo.ID);
        }
        foreach (var card in deck.discardPile)
        {
            deckData.discardPile.Add(card.CardInfo.ID);
        }
        foreach (var card in deck.hand)
        {
            deckData.hand.Add(card.CardInfo.ID);
        }
        foreach (var (card, turnsLeft) in deck.banishPile)
        {
            deckData.banishPile.Add((card.CardInfo.ID, turnsLeft));
        }
        foreach (var card in deck.inventoryCards)
        {
            deckData.inventoryCards.Add(card.CardInfo.ID);
        }
        return deckData;
    }

    public static Deck LoadDeck(DeckData data)
    {
        var deck = new Deck();
        foreach (var cardId in data.drawPile)
        {
            deck.drawPile.Add(CardFactory.GetCardByID(cardId));
        }
        foreach (var cardId in data.discardPile)
        {
            deck.discardPile.Add(CardFactory.GetCardByID(cardId));
        }
        foreach (var cardId in data.hand)
        {
            deck.hand.Add(CardFactory.GetCardByID(cardId));
        }
        foreach (var (cardId, turnsLeft) in data.banishPile)
        {
            deck.banishPile.Add((CardFactory.GetCardByID(cardId), turnsLeft));
        }
        foreach (var cardId in data.inventoryCards)
        {
            deck.inventoryCards.Add(CardFactory.GetCardByID(cardId));
        }
        return deck;
    }
}
