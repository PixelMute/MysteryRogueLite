using System.Collections.Generic;


/// <summary>
/// A card theme is a set of cards that all corespond to some mechanical theme.
/// </summary>
public class CardThemeHolder
{
    public string ThemeName { get; private set; } // Theme name cannot be changed once made.
    public List<Card> CardsInTheme { get; private set; }

    public CardThemeHolder(string name)
    {
        ThemeName = name;
        CardsInTheme = new List<Card>();
    }

    public void AddCardToTheme(Card c)
    {
        CardsInTheme.Add(c);
    }

    public int CountCardsInTheme()
    {
        return CardsInTheme.Count;
    }

    public Card GetRandomCardInTheme()
    {
        return CardsInTheme[Random.Range(0, CardsInTheme.Count)];
    }
}
