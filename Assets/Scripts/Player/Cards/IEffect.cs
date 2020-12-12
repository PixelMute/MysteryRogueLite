using System;
using System.Collections.Generic;
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

        public IEffect DamageFromEffect { get; private set; }

        public Strike(int damage, bool rawDamage = false) // if rawdamage is set, don't apply bonuses from things like status effects.
        {
            Damage = damage;
            RawDamage = rawDamage;
        }

        public Strike(IEffect damageEffect, bool rawDamage = false) // if rawdamage is set, don't apply bonuses from things like status effects.
        {
            DamageFromEffect = damageEffect;
            RawDamage = rawDamage;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            int newDamage;
            if (DamageFromEffect == null)
            {
                newDamage = Damage;
            }
            else
            {
                newDamage = DamageFromEffect.Activate(player, target);
            }

            if (!RawDamage) // If we want bonuses
            {
                int momentiumBonus = BattleManager.cardResolveStack.GetMomentumBonus();
                newDamage = (int)((newDamage + momentiumBonus) * BattleManager.cardResolveStack.GetInsightBonus());
            }

            return BattleGrid.instance.StrikeTile(target, player, newDamage);
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
            return BattleGrid.instance.StrikeTile(target, player, endDamage);
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
        public int StaticPower { get; private set; }
        public IEffect PowerFromEffect { get; private set; } = null;
        public BattleManager.StatusEffectEnum StatusEffect { get; private set; }
        public bool SelfTar { get; private set; }

        public ApplyStatusEffect(int initPower, BattleManager.StatusEffectEnum initStatusEffect, bool selfTar)
        {
            StaticPower = initPower;
            StatusEffect = initStatusEffect;
            SelfTar = selfTar;
        }

        public ApplyStatusEffect(IEffect powerFromEffect, BattleManager.StatusEffectEnum initStatusEffect, bool selfTar)
        {
            PowerFromEffect = powerFromEffect;
            StatusEffect = initStatusEffect;
            SelfTar = selfTar;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            Debug.Log("Apply status effect activated. " + StatusEffect.ToString() + ", " + StaticPower + ", self: " + SelfTar);
            int value;
            if (PowerFromEffect != null)
                value = PowerFromEffect.Activate(player, target);
            else
                value = StaticPower;

            if (SelfTar)
            {
                BattleManager.player.ApplyStatusEffect(StatusEffect, value);
                return 1;
            }
            else
            {
                bool flag = BattleGrid.instance.ApplyStatusEffectOnTile(target, StatusEffect, value);
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
        public IEffect EffectVal { get; private set; } = null;

        public DrawCards(int number)
        {
            Number = number;
        }

        public DrawCards(IEffect number)
        {
            EffectVal = number;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            int cardDrawAmount;

            if (EffectVal == null)
                cardDrawAmount = Number;
            else
                cardDrawAmount = EffectVal.Activate(player, target);

            Debug.Log("Drawing " + cardDrawAmount + " cards.");

            for (int i = 0; i < cardDrawAmount; i++)
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
        public enum CompareOperation { equals, lessThan, greaterThan, notEquals, max, min }
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
        public Heal(IEffect effect, bool healSelf = true)
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
                return BattleManager.player.TakeDamage(player, -1 * value);
            }
            else
            {
                return BattleGrid.instance.StrikeTile(player, target, -1 * value);
            }
        }
    }

    public class ManipulateHand : IEffect
    {
        public int StaticVal { get; private set; } = -1;
        public IEffect ValEffect { get; private set; } = null; // Get the value from this effect.

        public enum ManipulateHandTargetEnum { leftCard, rightCard, leftMostCard, rightMostCard, random };
        public enum ManipulateHandEffectEnum { discard, banish };

        public ManipulateHandTargetEnum Target { get; private set; } = ManipulateHandTargetEnum.leftCard;
        public ManipulateHandEffectEnum Effect { get; private set; } = ManipulateHandEffectEnum.discard;

        public ManipulateHand(int staticVal, ManipulateHandTargetEnum target, ManipulateHandEffectEnum effect)
        {
            StaticVal = staticVal;
            Target = target;
            Effect = effect;
        }
        public ManipulateHand(IEffect valeffect, ManipulateHandTargetEnum target, ManipulateHandEffectEnum effect)
        {
            ValEffect = valeffect;
            Target = target;
            Effect = effect;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            int value;
            if (ValEffect != null) // Use value from this effect
                value = ValEffect.Activate(player, target);
            else // Use static value
                value = StaticVal;

            if (BattleManager.cardResolveStack.MakeHandManipulateRequest(Target, Effect, value))
                return 1;
            else
                return 0;
        }
    }

    public class GetStatusEffectVal : IEffect
    {
        public BattleManager.StatusEffectEnum StatusEffect { get; private set; } // Get the value from this effect.
        public bool SelfTar { get; private set; }

        public GetStatusEffectVal(BattleManager.StatusEffectEnum effect, bool selfTar = true)
        {
            StatusEffect = effect;
            SelfTar = selfTar;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            if (SelfTar)
            {
                return BattleManager.player.GetStatusEffectValue(StatusEffect);
            }
            else
            {
                TileCreature tc = BattleManager.instance.map.map[target.x, target.y].GetEntityOnTile() as TileCreature;
                if (tc != null)
                    return tc.GetStatusEffectValue(StatusEffect);
                else
                    return 0;
            }
        }
    }

    // Gets some value about the player and returns it in the activation effect.
    public class GetPlayerValue : IEffect
    {
        public enum GetPlayerValueEnum { cardsInHand, cardsInDraw, cardsInDiscard, cardsInBanish, health, energy};
        public GetPlayerValueEnum TargetedVal { get; private set; }

        public GetPlayerValue(GetPlayerValueEnum tar)
        {
            TargetedVal = tar;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            switch (TargetedVal)
            {
                case GetPlayerValueEnum.cardsInHand:
                    return BattleManager.player.playerDeck.hand.Count;
                case GetPlayerValueEnum.cardsInDraw:
                    return BattleManager.player.playerDeck.drawPile.Count;
                case GetPlayerValueEnum.cardsInDiscard:
                    return BattleManager.player.playerDeck.discardPile.Count;
                case GetPlayerValueEnum.cardsInBanish:
                    return BattleManager.player.playerDeck.banishPile.Count;
                case GetPlayerValueEnum.health:
                    return BattleManager.player.Health;
                case GetPlayerValueEnum.energy:
                    Debug.Log("Energy gaN");
                    return BattleManager.player.CurrentEnergy;
                default:
                    return 0;
            }
        }
    }

    // Repeats an effect a certain number of times, up to a max.
    // Can also be used as an if statement if max loops is set to 1.
    public class LoopEffect : IEffect
    {
        public IEffect EffectValue { get; private set; }
        public int StaticValue { get; private set; }

        public int MaxLoopCount { get; private set; } // -1 for unlimited.


        public IEffect LoopedEffect { get; private set; }

        public LoopEffect(IEffect looped, IEffect valueEffect, int max = -1)
        {
            EffectValue = valueEffect;
            LoopedEffect = looped;
            MaxLoopCount = max;
        }

        // If you're looping a static number of times, probably don't need max.
        public LoopEffect(IEffect looped, int staticVal, int max = -1)
        {
            MaxLoopCount = max;
            StaticValue = staticVal;
            LoopedEffect = looped;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            int maxLoopAmount;

            if (EffectValue == null)
                maxLoopAmount = StaticValue;
            else
                maxLoopAmount = EffectValue.Activate(player, target);

            if (MaxLoopCount > -1)
                maxLoopAmount = Math.Min(MaxLoopCount, maxLoopAmount); // This acts as a ceiling.

            int sum = 0;
            for (int i = 0; i < maxLoopAmount; i++)
            {
                sum += LoopedEffect.Activate(player, target);
            }
            return sum;
        }
    }

    public class GainSpirit : IEffect
    {
        public int StaticVal { get; set; } = -1;
        public IEffect DamageFromEffect { get; set; } = null; // Get the value from this effect.
        // Only the player has spirit, so there's no targeting.

        public GainSpirit(int staticVal)
        {
            StaticVal = staticVal;
        }
        public GainSpirit(IEffect effect)
        {
            DamageFromEffect = effect;
        }

        public int Activate(Vector2Int player, Vector2Int target)
        {
            int value;
            if (DamageFromEffect != null) // Use value from this effect
                value = DamageFromEffect.Activate(player, target);
            else // Use static value
                value = StaticVal;

            BattleManager.player.GainSpirit(value);
            return 1;
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

