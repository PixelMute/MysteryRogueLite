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

public static class EffectFactory
{
    public static IEffect GetEffectFromString(string effect)
    {
        var split = effect.ToLower().Split(':');
        if (split.Length == 2 && int.TryParse(split[1], out var val))
        {
            switch (split[0])
            {
                case "damage":
                    return new Strike(val);
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

