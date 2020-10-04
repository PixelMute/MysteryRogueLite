using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public enum PlayCondition { needsLOS, straightLine, emptyTile, cornerCutting, mustHitCreature };

public class Card
{
    public CardInfo CardInfo { get; private set; }
    public List<IEffect> Effects { get; private set; }
    public Range Range { get; private set; }

    public TileCreature Owner { get; set; }

    //public Sprite Sprite { get; private set; }  //For when we have different sprites for each card
    //public ICardAnimation Animation { get; private set; } //For when we have different animations for each card

    public Card(CardInfo cardInfo, Range range) : this(cardInfo, range, new List<IEffect>()) { }

    public Card(CardInfo cardInfo, Range range, List<IEffect> effects)
    {
        Effects = effects;
        CardInfo = cardInfo;
        Range = range;
    }

    public void AddEffect(IEffect effect)
    {
        Effects.Add(effect);
    }

    public bool TargetWithinRange(Vector2Int player, Vector2Int target)
    {
        if (Range == null)
        {
            return false;
        }
        else return Range.IsTargetInRange(player, target);
    }

    /// <summary>
    /// Activate the cards effect
    /// </summary>
    /// <param name="player">The player that played the card</param>
    /// <param name="target">The target of the card</param>
    public void Activate(Vector2Int player, Vector2Int target)
    {
        foreach (var effect in Effects)
        {
            effect.Activate(player, target);
        }
        //TODO: Add animation to this 
    }
}

public class CardInfo
{
    public int SpiritCost { get; set; }
    public int EnergyCost { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int ID { get; set; }
}

