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
    public int Damage { get; private set; }

    public Strike(int damage)
    {
        Damage = damage;
    }

    public void Activate(Vector2Int player, Vector2Int target)
    {
        BattleGrid.instance.StrikeTile(target, Damage);
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
        BattleGrid.instance.StrikeTile(target, BattleManager.player.GetStatusEffectValue(StatusEffect));
    }
}

public class Teleport : IEffect
{
    //Teleports player to target
    public void Activate(Vector2Int player, Vector2Int target)
    {
        BattleManager.player.MoveTo(target, true);
    }
}

//Might want to make this a more generic class to apply different status effects
//Upgrades the players defence by the given amount
public class UpgradeDefence : IEffect
{
    public int Defence { get; private set; }

    public UpgradeDefence(int defence)
    {
        Defence = defence;
    }

    public void Activate(Vector2Int player, Vector2Int target)
    {
        BattleManager.player.ApplyStatusEffect(BattleManager.StatusEffectEnum.defence, 12);
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
                    return new Strike(val);
                case "upgradedefence":
                    return new UpgradeDefence(val);
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
        return null;
    }

    public static List<IEffect> GetListOfEffectsFromString(string effects)
    {
        var effectList = effects.ToLower().Split(';');
        var res = new List<IEffect>();
        foreach (var effect in effectList)
        {
            res.Add(GetEffectFromString(effect));
        }
        return res;
    }
}

