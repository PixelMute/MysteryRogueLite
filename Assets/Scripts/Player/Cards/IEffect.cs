using Roguelike;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Roguelike
{
    public interface IEffect
    {
        int Activate(Vector2Int player, Vector2Int target);
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

        public int Activate(Vector2Int player, Vector2Int target)
        {
            int newDamage = Damage;
            if (!RawDamage) // If we want bonuses
            {
                int momentiumBonus = BattleManager.cardResolveStack.GetMomentumBonus();
                newDamage = (int)((Damage + momentiumBonus) * BattleManager.cardResolveStack.GetInsightBonus());
            }

            return BattleGrid.instance.StrikeTile(target, newDamage);
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

        public int Activate(Vector2Int player, Vector2Int target)
        {
            // Apply momentium bonus
            int momentiumBonus = BattleManager.cardResolveStack.GetMomentumBonus();
            float insightBonus = BattleManager.cardResolveStack.GetInsightBonus();
            int endDamage = (int)((BattleManager.player.GetStatusEffectValue(StatusEffect) + momentiumBonus) * insightBonus);
            return BattleGrid.instance.StrikeTile(target, endDamage);
        }
    }

    /// <summary>
    /// Moves player to target tile
    /// </summary>
    public class Teleport : IEffect
    {
        //Teleports player to target
        public int Activate(Vector2Int player, Vector2Int target)
        {
            BattleManager.player.MoveToPosition(target);
            return 0;
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

        public int Activate(Vector2Int player, Vector2Int target)
        {
            Debug.Log("Apply status effect activated. " + StatusEffect.ToString() + ", " + Power + ", self: " + SelfTar);
            if (SelfTar)
            {
                BattleManager.player.ApplyStatusEffect(StatusEffect, Power);
                return 1;
            }
            else
            {
                bool flag = BattleGrid.instance.ApplyStatusEffectOnTile(target, StatusEffect, Power);
                if (flag) return 1; else return 0;
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
        public bool HitEmpty { get; private set; }

        public AOEAttack(IEffect appliedEffect, int radius, bool tarCentered, bool hitEmpty)
        {
            AppliedEffect = appliedEffect;
            Radius = radius;
            TarCentered = tarCentered;
            HitEmpty = hitEmpty;
        }

        /// <returns>Returns the number of targets hit.</returns>
        public int Activate(Vector2Int player, Vector2Int target)
        {
            if (!TarCentered)
            {
                return EffectFactory.SquareAOE(Radius, target, player, true, AppliedEffect, HitEmpty);
            }
            else
            {
                return EffectFactory.SquareAOE(Radius, player, player, false, AppliedEffect, HitEmpty);
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

        public int Activate(Vector2Int player, Vector2Int target)
        {
            for (int i = 0; i < Number; i++)
            {
                BattleManager.player.DrawCard();
            }
            return Number;
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

        public int Activate(Vector2Int player, Vector2Int target)
        {
            BattleManager.player.AddEnergy(Number);
            return Number;
        }
    }

    //A 'utility' effect whose Activate function will do some operation on two values.
    // Can be given either two effects or an effect and an int.
    // And int takes priority over an effect.
    public class Var_CompareOp : IEffect
    {
        public enum CompareOperation { equals, lessThan, greaterThan, notEquals, max, min}
        private CompareOperation op;
        public int ConstantInt { get; private set; }
        private IEffect EffectA { get; set; }
        private IEffect EffectB { get; set; }

        public Var_CompareOp(IEffect effectA, IEffect effectB, CompareOperation op)
        {
            EffectA = effectA;
            EffectB = effectB;
            this.op = op;
        }
        public Var_CompareOp(IEffect effectA, int con, CompareOperation op)
        {
            EffectA = effectA;
            EffectB = null;
            ConstantInt = con;
            this.op = op;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            int valA = EffectA.Activate(player, target);
            int valB;

            if (EffectB == null)
                valB = ConstantInt;
            else
                valB = EffectB.Activate(player, target);

            switch (op)
            {
                case CompareOperation.equals:
                    if (valA == valB) return 1; else return 0;
                case CompareOperation.greaterThan:
                    if (valA > valB) return 1; else return 0;
                case CompareOperation.lessThan:
                    if (valA < valB) return 1; else return 0;
                case CompareOperation.notEquals:
                    if (valA != valB) return 1; else return 0;
                case CompareOperation.max:
                    return Math.Max(valA, valB);
                case CompareOperation.min:
                    return Math.Min(valA, valB);
                default:
                    return 0;
            }
        }
    }

    public class Heal : IEffect
    {
        public int StaticVal { get; set; } = -1;
        public IEffect DamageFromEffect { get; set; } = null; // Get the value from this effect.
        public bool healSelf = true; // If true, always heal the player. Otherwise, heal the target.

        public Heal(int staticVal, bool healSelf = true)
        {
            StaticVal = staticVal;
            this.healSelf = healSelf;
        }
        public Heal (IEffect effect, bool healSelf = true)
        {
            DamageFromEffect = effect;
            this.healSelf = healSelf;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            int value;
            if (DamageFromEffect != null) // Use value from this effect
                value = DamageFromEffect.Activate(player, target);
            else // Use static value
                value = StaticVal;

            Debug.Log("Heal effect activated with a value of " + value);

            if (healSelf)
            {
                return BattleManager.player.TakeDamage(-1 * value);
            }
            else
            {
                return BattleGrid.instance.StrikeTile(target, -1 * value);
            }
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
                            return new AOEAttack(new Strike(power, true), radius, tarCentered, false);
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
                //Debug.Log("Parsing " + effect);
                res.Add(GetEffectFromString(effect));
            }
            return res;
        }

        /// <summary>
        /// Applies an effect in a square. Ignores LOS.
        /// </summary>
        /// <param name="radius">How many blocks out to effect. EG: radius of 1 is a 3x3 effect.</param>
        /// <param name="center">coords for the center of the AOE effect.</param>
        /// <param name="player">coords for where the player is.</param>
        /// <param name="hitMiddle">Should we apply the effect to the middle tile?</param>
        /// <param name="appliedEffect">What effect to apply</param>
        /// <param name="hitEmpty">Apply the effect even if no creature is there?</param>
        /// <returns>Returns the number of targets hit. Note that if you have hit empty enabled, this will just count tiles.</returns>
        public static int SquareAOE(int radius, Vector2Int center, Vector2Int player, bool hitMiddle, IEffect appliedEffect, bool hitEmpty)
        {
            // Want to apply bonus damage once
            Strike s = appliedEffect as Strike;
            int oldDamage = 0;
            if (s != null)
            {
                oldDamage = s.Damage;
                s.Damage = (int)((s.Damage + BattleManager.cardResolveStack.GetMomentumBonus()) * BattleManager.cardResolveStack.GetInsightBonus());
            }

            int targetsHit = 0;

            for (int xOffset = -radius; xOffset <= radius; xOffset++)
            {
                for (int yOffset = -radius; yOffset <= radius; yOffset++)
                {
                    if (!(xOffset == 0 && yOffset == 0) || hitMiddle)
                    {
                        try
                        {
                            if (hitEmpty || BattleManager.instance.map.map[center.x + xOffset, center.y + yOffset].IsCreatureOnTile())
                            {
                                targetsHit++;
                                Debug.Log("AOE effect triggered. Offset = " + xOffset + ", " + yOffset + ". Hitmiddle: " + hitMiddle);
                                appliedEffect.Activate(player, new Vector2Int(center.x + xOffset, center.y + yOffset));
                            }

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

            return targetsHit;
        }
    }
}

