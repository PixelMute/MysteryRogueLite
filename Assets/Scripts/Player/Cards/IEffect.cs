using Roguelike;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public interface IEffect
{
    void Activate(Vector2Int player, Vector2Int target);
}

//Damages the enemy on the given target
public class Strike : IEffect
{
    public int Damage { get; set; }
    public bool RawDamage { get; private set; }



    public Strike(int damage, bool rawDamage = false) // if rawdamage is set, don't apply bonuses from things like status effects.
    {
        Damage = damage;
        RawDamage = rawDamage;
    }

    public void Activate(Vector2Int player, Vector2Int target)
    {
        int newDamage = Damage;
        if (!RawDamage) // If we want bonuses
        {
            int momentiumBonus = BattleManager.cardResolveStack.GetMomentumBonus();
            newDamage = (int)((Damage + momentiumBonus) * BattleManager.cardResolveStack.GetInsightBonus());
        }

        BattleGrid.instance.StrikeTile(target, newDamage);
    }
}

//Deals damage based on status effect it is given
public class DamageBasedOnStatus : IEffect
{
    public BattleManager.StatusEffectEnum StatusEffect { get; private set; }
    public DamageBasedOnStatus(BattleManager.StatusEffectEnum effect)
    {
        StatusEffect = effect;
    }

    public void Activate(Vector2Int player, Vector2Int target)
    {
        // Apply momentium bonus
        int momentiumBonus = BattleManager.cardResolveStack.GetMomentumBonus();
        float insightBonus = BattleManager.cardResolveStack.GetInsightBonus();
        BattleGrid.instance.StrikeTile(target, (int)((BattleManager.player.GetStatusEffectValue(StatusEffect) + momentiumBonus) * insightBonus));
    }
}

/// <summary>
/// Moves player to target tile
/// </summary>
public class Teleport : IEffect
{
    //Teleports player to target
    public void Activate(Vector2Int player, Vector2Int target)
    {
        BattleManager.player.MoveTo(target, true);
    }
}

/// <summary>
/// Applys a given status effect to whatever creature is on the target tile.
/// </summary>
public class ApplyStatusEffect : IEffect
{
    public int Power { get; private set; }
    public BattleManager.StatusEffectEnum StatusEffect { get; private set; }
    public bool SelfTar { get; private set; }

    public ApplyStatusEffect(int initPower, BattleManager.StatusEffectEnum initStatusEffect, bool selfTar)
    {
        Power = initPower;
        StatusEffect = initStatusEffect;
        SelfTar = selfTar;
    }

    public void Activate(Vector2Int player, Vector2Int target)
    {
        Debug.Log("Apply status effect activated. " + StatusEffect.ToString() + ", " + Power + ", self: " + SelfTar);
        if (SelfTar)
        {
            BattleManager.player.ApplyStatusEffect(StatusEffect, Power);
        }
        else
        {
            BattleGrid.instance.ApplyStatusEffectOnTile(target, StatusEffect, Power);
        }
        
    }
}

/// <summary>
/// Applies effect in an AOE. Can set wether it is centered on target or player, and if it should affect middle.
/// </summary>
public class AOEAttack : IEffect
{
    public IEffect AppliedEffect { get; private set; }
    public int Radius { get; private set; }
    public bool TarCentered { get; private set; }

    public AOEAttack(IEffect appliedEffect, int radius, bool tarCentered)
    {
        AppliedEffect = appliedEffect;
        Radius = radius;
        TarCentered = tarCentered;
    }

    public void Activate(Vector2Int player, Vector2Int target)
    {
        if (TarCentered)
        {
            EffectFactory.SquareAOE(Radius, target, player, true, AppliedEffect);
        }
        else
        {
            EffectFactory.SquareAOE(Radius, player, player, false, AppliedEffect);
        }
    }
}

/// <summary>
/// Lets the player draw cards.
/// </summary>
public class DrawCards : IEffect
{
    public int Number { get; private set; }

    public DrawCards(int number)
    {
        Number = number;
    }

    public void Activate(Vector2Int player, Vector2Int target)
    {
        for (int i = 0; i < Number; i++)
        {
            BattleManager.player.DrawCard();
        }
    }
}

/// <summary>
/// Gives the player the specified amount of energy
/// </summary>
public class GainEnergy : IEffect
{
    public int Number { get; private set; }

    public GainEnergy(int number)
    {
        Number = number;
    }

    public void Activate(Vector2Int player, Vector2Int target)
    {
        BattleManager.player.AddEnergy(Number);
    }
}

public static class EffectFactory
{
    //Gets the effect based on the string in the csv file
    public static IEffect GetEffectFromString(string effect)
    {
        var split = effect.ToLower().Split(':');
        if (split.Length == 2 && int.TryParse(split[1], out var val))
        {
            //For effects that need a value
            switch (split[0])
            {
                case "strike":
                    return new Strike(val, false);
                case "drawcards":
                    return new DrawCards(val);
                case "gainenergy":
                    return new GainEnergy(val);
            }
        }
        else if (split.Length == 1)
        {
            //For effects that don't need a value
            switch (split[0])
            {
                case "teleport":
                    return new Teleport();
            }
        }
        else if (split.Length == 2 && Enum.TryParse(split[1], out BattleManager.StatusEffectEnum res))
        {
            //For effects that use the status effect enum
            switch (split[0])
            {
                case "damagebasedonstatus":
                    return new DamageBasedOnStatus(res);
            }
        }
        else if (split.Length == 3 && int.TryParse(split[1], out var statusPower) && Enum.TryParse(split[2], out BattleManager.StatusEffectEnum statusEnum))
        {
            // Effects that take an int and a status effect.
            switch (split[0])
            {
                case "applystatuseffect":
                    return new ApplyStatusEffect(statusPower, statusEnum, false);
                case "applystatuseffecttoself":
                    return new ApplyStatusEffect(statusPower, statusEnum, true);
            }
        }
        else if (split.Length == 4) // Something that takes 3 args
        {
            switch (split[0])
            {
                case "aoedamage": // 0 is 'aoe', 1 is damage, 2 is radius, 3 is true if it is centered on the target, false if centered on player. Attacks centered on the player do not hurt the player.
                    if (int.TryParse(split[1], out var power) && int.TryParse(split[2], out var radius) && bool.TryParse(split[3], out var tarCentered))
                    { // Raw damage is set to true for this strike so that it counts as a single instance of damage.
                        return new AOEAttack(new Strike(power, true), radius, tarCentered);
                    }
                    else
                    {
                        return null;
                    }
            }
        }
        return null;
    }

    public static List<IEffect> GetListOfEffectsFromString(string effects)
    {
        var effectList = effects.ToLower().Split(';');
        var res = new List<IEffect>();
        foreach (var effect in effectList)
        {
            Debug.Log("Parsing " + effect);
            res.Add(GetEffectFromString(effect));
        }
        return res;
    }

    public static void SquareAOE(int radius, Vector2Int center, Vector2Int player, bool hitMiddle, IEffect appliedEffect)
    {
        // Want to apply bonus damage once
        Strike s = appliedEffect as Strike;
        int oldDamage = 0;
        if (s != null)
        {
            oldDamage = s.Damage;
            s.Damage = (int)((s.Damage + BattleManager.cardResolveStack.GetMomentumBonus()) * BattleManager.cardResolveStack.GetInsightBonus());
        }

        for (int xOffset = -radius; xOffset <= radius; xOffset++)
        {
            for (int yOffset = -radius; yOffset <= radius; yOffset++)
            {
                if (!(xOffset == 0 && yOffset == 0) || hitMiddle)
                {
                    try
                    {
                        appliedEffect.Activate(player, new Vector2Int(center.x + xOffset, center.y + yOffset));
                    }
                    catch (Exception)
                    {
                        // Outside of map. Do nothing.
                    }
                }

            }
        }

        if (s != null)
        {
            s.Damage = oldDamage;
        }
    }
}

