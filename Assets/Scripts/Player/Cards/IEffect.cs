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

public class Teleport : IEffect
{
    //Teleports player to target
    public void Activate(Vector2Int player, Vector2Int target)
    {
        BattleManager.player.MoveTo(target, true);
    }
}

public static class EffectFactory
{
    public static IEffect GetEffectFromString(string effect)
    {
        var split = effect.ToLower().Split(':');
        if (split.Length == 2 && int.TryParse(split[1], out var val))
        {
            //For effects that need a value
            switch (split[0])
            {
                case "damage":
                    return new Strike(val);
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

