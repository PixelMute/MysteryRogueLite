using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Roguelike;

using System.Xml;

public class CardFactory
{
    private static Dictionary<string, Card> CardDictionary;
    private static Dictionary<string, CardThemeHolder> ThemeDictionary;

    /// <summary>
    /// Loads the card information from the csv file
    /// </summary>
    public static void LoadCards()
    {
        /*var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            LineBreakInQuotedFieldIsBadData = false,     //Set so we don't get bad data if there is line break in description
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,     //Trim excess white space
        };
        var fileName = Application.dataPath + "/StreamingAssets/cardinfo.csv";
        try
        {
            using (var fileReader = new StreamReader(fileName))
            {
                using (var csv = new CsvReader(fileReader, config, false))
                {
                    CreateCardDictionary(csv);
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error caught: " + e.Message);
            throw new Exception("Unable to open card info file.", e);
        }*/
        CreateCardDictionaryXML();
    }

    //Creates the dictionary to store the cards in memory
    private static void CreateCardDictionary(CsvReader csv)
    {
        CardDictionary = new Dictionary<string, Card>();
        csv.Read();
        csv.ReadHeader();
        while (csv.Read())
        {
            var cardInfo = new CardInfo()
            {
                ID = csv.GetField<int>("ID"),
                SpiritCost = csv.GetField<int>("SpiritCost"),
                EnergyCost = csv.GetField<int>("EnergyCost"),
                Name = csv.GetField<string>("Name"),
                Description = csv.GetField<string>("Description"),
            };
            var effectString = csv.GetField<string>("Effects");
            var effectList = EffectFactory.GetListOfEffectsFromString(effectString);
            var minRange = csv.GetField<int>("MinRange");
            var maxRange = csv.GetField<int>("MaxRange");

            var conditions = RangeFactory.GetPlayConditionsFromString(csv.GetField<string>("PlayConditions"));
            var range = new Range(conditions, minRange, maxRange);
            var card = new Card(cardInfo, range, effectList);
            //Debug.Log("Adding card: " + cardInfo.Name.ToLower());
            CardDictionary.Add(cardInfo.Name.ToLower(), card);
        }
    }

    public static void CreateCardDictionaryXML()
    {
        CardDictionary = new Dictionary<string, Card>();
        ThemeDictionary = new Dictionary<string, CardThemeHolder>();

        var fileName = Application.dataPath + "/StreamingAssets/cardinfo.xml";
        try
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(fileName);
            foreach (XmlNode node in doc.GetElementsByTagName("card"))
            { // This is for each card.
                Card newCard = ParseCardXML(node);
                CardDictionary.Add(newCard.CardInfo.Name.ToLower(), newCard);
                Debug.Log("Card parsed: " + newCard.CardInfo.Name + ", which is in theme: " + newCard.CardInfo.ThemeName);

                // Also make sure to add it in the theme dictionary if it has a theme
                // If that theme already exists, add it.
                if (newCard.CardInfo.ThemeName != null)
                {
                    if (ThemeDictionary.TryGetValue(newCard.CardInfo.ThemeName, out CardThemeHolder cth))
                        cth.AddCardToTheme(newCard);
                    else
                    {
                        // Theme does not exist. Add a new theme.
                        Debug.Log("Adding a new theme called " + newCard.CardInfo.ThemeName + " for card named " + newCard.CardInfo.Name);
                        CardThemeHolder newTheme = new CardThemeHolder(newCard.CardInfo.ThemeName);
                        newTheme.AddCardToTheme(newCard);
                        ThemeDictionary.Add(newTheme.ThemeName, newTheme);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error caught: " + e.Message);
            throw new Exception("Unable to open card info file.", e);
        }
    }

    private static Card ParseCardXML(XmlNode node)
    {
        var cardInfo = new CardInfo();
        int minRange = 0;
        int maxRange = 0;
        List<IEffect> effectList = null;
        List<IEffect> onBanishEffects = null;
        List<IEffect> onDiscardEffects = null;
        List<PlayCondition> playConditionList = null;

        foreach (XmlNode subnode in node.ChildNodes)
        {
            //Debug.Log("In subnode. Name = " + subnode.Name);
            switch (subnode.Name)
            {
                case "id":
                    if (int.TryParse(subnode.Attributes["val"]?.Value, out int a))
                        cardInfo.ID = a;
                    else
                        throw new Exception("Cannot parse id.");
                    break;
                case "spiritcost":
                    if (int.TryParse(subnode.Attributes["val"]?.Value, out int b))
                        cardInfo.SpiritCost = b;
                    else
                        throw new Exception("Cannot parse spiritcost.");
                    break;
                case "energycost":
                    if (int.TryParse(subnode.Attributes["val"]?.Value, out int c))
                        cardInfo.EnergyCost = c;
                    else
                        throw new Exception("Cannot parse energycost.");
                    break;
                case "name":
                    cardInfo.Name = subnode.InnerText;
                    break;
                case "description":
                    cardInfo.Description = subnode.InnerText;
                    break;
                case "minrange":
                    if (int.TryParse(subnode.Attributes["val"]?.Value, out int d))
                        minRange = d;
                    else
                        throw new Exception("Cannot parse minrange.");
                    break;
                case "maxrange":
                    if (int.TryParse(subnode.Attributes["val"]?.Value, out int e))
                        maxRange = e;
                    else
                        throw new Exception("Cannot parse maxrange.");
                    break;
                case "effects":
                    effectList = ParseEffects(subnode);
                    break;
                case "banisheffects":
                    onBanishEffects = ParseEffects(subnode);
                    break;
                case "discardeffects":
                    onDiscardEffects = ParseEffects(subnode);
                    break;
                case "playconditions":
                    playConditionList = ParsePlayConditions(subnode);
                    break;
                case "theme":
                    cardInfo.ThemeName = subnode.InnerText;
                    break;
                case "banishafterplay":
                    cardInfo.ResolveBehavior = CardInfo.ResolveBehaviorEnum.banish;
                    if (int.TryParse(subnode.Attributes["val"]?.Value, out int f))
                        cardInfo.BanishAmount = f;
                    else
                        throw new Exception("Cannot parse BanishAfterPlay");
                    break;
                default:
                    Debug.LogWarning("CardFactory::ParseCardXML() -- Unknown node: " + subnode.Name);
                    break;
            }
        }

        if (playConditionList == null || effectList == null)
        {
            throw new Exception("CardFactory::ParseCardXML() -- Card named " + cardInfo.Name + " does not have play conditions and/or effect list.");
        }

        // Now that we've done that, compile the info into an actual card.
        var range = new Range(playConditionList, minRange, maxRange);
        var card = new Card(cardInfo, range, effectList, onDiscardEffects, onBanishEffects);

        return card;
    }

    private static List<PlayCondition> ParsePlayConditions(XmlNode conditionsNode)
    {
        XmlNodeList subnodes = conditionsNode.ChildNodes;
        List<PlayCondition> conditionsList;
        if (subnodes != null)
        {
            conditionsList = new List<PlayCondition>(subnodes.Count);
            foreach (XmlNode effect in subnodes)
            {
                string lowerName = effect.Name.ToLower();
                switch (lowerName)
                {
                    case "needslos":
                        conditionsList.Add(PlayCondition.needsLOS);
                        break;
                    case "musthitcreature":
                        conditionsList.Add(PlayCondition.mustHitCreature);
                        break;
                    case "straightline":
                        conditionsList.Add(PlayCondition.straightLine);
                        break;
                    case "emptytile":
                        conditionsList.Add(PlayCondition.emptyTile);
                        break;
                    case "cornercutting":
                        conditionsList.Add(PlayCondition.cornerCutting);
                        break;
                    default:
                        Debug.LogWarning("Unknown card PlayCondition with the name " + effect.Name);
                        break;
                }
            }
        }
        else
            conditionsList = new List<PlayCondition>(0); // No play conditions

        return conditionsList;
    }

    private static List<IEffect> ParseEffects(XmlNode effectsNode)
    {
        XmlNodeList subnodes = effectsNode.ChildNodes;
        List<IEffect> effectList;
        if (subnodes != null)
        {
            effectList = new List<IEffect>(subnodes.Count);
            foreach (XmlNode effect in subnodes)
            {
                effectList.Add(ParseSingleEffect(effect));
            }
        }
        else
            effectList = new List<IEffect>(0); // No effects

        return effectList;
    }

    /// <summary>
    /// Returns the object representing this effect
    /// </summary>
    /// <param name="effect"></param>
    /// <returns></returns>
    private static IEffect ParseSingleEffect(XmlNode effect)
    {
        string lowerName = effect.Name.ToLower();
        switch (lowerName)
        {
            case "strike":
                return ParseStrike(effect);
            case "drawcards":
                return ParseDrawCards(effect);
            case "gainenergy":
                return ParseGainEnergy(effect);
            case "teleport":
                return ParseTeleport(effect);
            case "damagebasedonstatus":
                return ParseDamageBasedOnStatusEffect(effect);
            case "applystatuseffect":
                return ParseApplyStatusEffect(effect);
            case "aoe":
                return ParseAOE(effect);
            case "var_compareop":
                return ParseVarCompareOp(effect);
            case "heal":
                return ParseHeal(effect);
            case "maniphand":
                return ParseManipHand(effect);
            default:
                Debug.LogWarning("Unknown card effect with the name " + effect.Name);
                return null;
        }
    }


    #region ParsingIndividualEffects
    private static IEffect ParseStrike(XmlNode effectNode)
    {
        if (!int.TryParse(effectNode.Attributes["dmg"]?.Value, out int damage))
        {
            damage = 10; // Default
            Debug.LogWarning("No value 'dmg' found for this strike node. Using default value.");
        }

         
        if (!bool.TryParse(effectNode.Attributes["rawdamage"]?.Value, out bool rawDamage))
            rawDamage = false;
        return new Strike(damage, rawDamage);
    }

    private static IEffect ParseDrawCards(XmlNode effectNode)
    {
        if (!int.TryParse(effectNode.Attributes["val"]?.Value, out int num))
        {
            Debug.LogWarning("No value 'val' found for this drawcards node. Using default value.");
            num = 1;
        }
        return new DrawCards(num);
    }

    private static IEffect ParseGainEnergy(XmlNode effectNode)
    {
        if (!int.TryParse(effectNode.Attributes["val"]?.Value, out int num))
        {
            Debug.LogWarning("No value 'val' found for this gainenergy node. Using default value.");
            num = 1;
        }
        return new GainEnergy(num);
    }

    private static IEffect ParseTeleport(XmlNode effect)
    {
        return new Teleport(); // Maybe later we'll need this.
    }

    private static IEffect ParseDamageBasedOnStatusEffect(XmlNode effect)
    {
        if (!Enum.TryParse(effect.Attributes["status"]?.Value, out BattleManager.StatusEffectEnum res))
        {
            Debug.LogWarning("No value 'status' found for this damagebasedonstatuseffect node. Using default value.");
            res = BattleManager.StatusEffectEnum.defense;
        }
        return new DamageBasedOnStatus(res);
    }

    // <applystatuseffect status="momentum" power="7" self="true">
    //  <internalEffect/> (if not using a power)
    // </applystatuseffect>
    private static IEffect ParseApplyStatusEffect(XmlNode effect)
    {
        if (!Enum.TryParse(effect.Attributes["status"]?.Value, out BattleManager.StatusEffectEnum res))
        {
            Debug.LogWarning("No value 'status' found for this applystatuseffect node. Using default value.");
            res = BattleManager.StatusEffectEnum.defense;
        }

        if (!bool.TryParse(effect.Attributes["self"]?.Value, out bool self))
        {
            Debug.LogWarning("No value 'self' found for this applystatuseffect node. Using default value.");
            self = true;
        }

        if (!int.TryParse(effect.Attributes["power"]?.Value, out int power))
        {
            XmlNodeList subnodes = effect.ChildNodes;
            if (subnodes != null && subnodes.Count == 1)
            {
                return new ApplyStatusEffect(ParseSingleEffect(subnodes[0]), res, self);
            }
            else
                throw new Exception("CardFactory::ParseAOE(" + effect.Name + ") -- Wrong number of subeffects. Expected 1");
        }

        return new ApplyStatusEffect(power, res, self);
    }

    private static IEffect ParseAOE(XmlNode effect)
    {
        // First, get the subeffect.
        IEffect subeffect;

        XmlNodeList subnodes = effect.ChildNodes;
        if (subnodes != null && subnodes.Count == 1)
        {
            subeffect = ParseSingleEffect(subnodes[0]);
        }
        else
            throw new Exception("CardFactory::ParseAOE(" + effect.Name + ") -- Wrong number of subeffects. Expected 1, got " + subnodes.Count + " instead.");

        // Now we need to get all the other values.
        if (!bool.TryParse(effect.Attributes["selftar"]?.Value, out bool selftar))
        {
            selftar = true;
        }

        if (!bool.TryParse(effect.Attributes["hitempty"]?.Value, out bool hitempty))
        {
            hitempty = false;
        }

        if (!int.TryParse(effect.Attributes["radius"]?.Value, out int radius))
        {
            Debug.LogWarning("No value 'radius' found for this ParseAOE node. Using default value.");
            radius = 1;
        }

        return new AOEAttack(subeffect, radius, selftar, hitempty);
    }

    // <var_compareop op="+" const="11">
    // <effectA/>
    // <effectB/> (if you don't have a const)
    // </var_compareop>
    private static IEffect ParseVarCompareOp(XmlNode effect)
    {
        bool usingConst = int.TryParse(effect.Attributes["const"]?.Value, out int constant);

        Var_CompareOp.CompareOperation op;

        // Find operation
        if (effect.Attributes["op"] != null)
        {
            string opToParse = effect.Attributes["op"].Value;
            switch (opToParse)
            {
                case "==":
                case "=":
                    op = Var_CompareOp.CompareOperation.equals;
                    break;
                case "<":
                    op = Var_CompareOp.CompareOperation.lessThan;
                    break;
                case ">":
                    op = Var_CompareOp.CompareOperation.greaterThan;
                    break;
                case "!=":
                    op = Var_CompareOp.CompareOperation.notEquals;
                    break;
                case "max":
                    op = Var_CompareOp.CompareOperation.max;
                    break;
                case "min":
                    op = Var_CompareOp.CompareOperation.min;
                    break;
                default:
                    Debug.LogWarning("Unknown value " + opToParse + " for op. Using default value of '=='.");
                    op = Var_CompareOp.CompareOperation.equals;
                    break;
            }
        }
        else
        {
            Debug.LogWarning("No value 'op' found for this var_compareop node. Using default value of '=='.");
            op = Var_CompareOp.CompareOperation.equals;
        }

        // Now, we need to parse the effect, or two effects if we're not using a constant.
        XmlNodeList subnodes = effect.ChildNodes;
        if (subnodes == null)
        {
            throw new Exception("CardFactory::ParseVarCompareOp(" + effect.Name + ") -- Wrong number of subeffects. Got null.");
        }

        if (usingConst)
        {
            if (subnodes.Count != 1)
                throw new Exception("CardFactory::ParseVarCompareOp(" + effect.Name + ") -- Wrong number of subeffects. Expected 1, but got " + subnodes.Count);

            return new Var_CompareOp(ParseSingleEffect(subnodes[0]), constant, op);
        }
        else
        {
            if (subnodes.Count != 2)
                throw new Exception("CardFactory::ParseVarCompareOp(" + effect.Name + ") -- Wrong number of subeffects. Expected 2, but got " + subnodes.Count);

            return new Var_CompareOp(ParseSingleEffect(subnodes[0]), ParseSingleEffect(subnodes[1]), op);
        }
    }

    //<heal val="11" selfTar="true">
    // <effectA/> (if you don't have a const)
    // </heal>
    private static IEffect ParseHeal(XmlNode effect)
    {
        bool usingConst = int.TryParse(effect.Attributes["val"]?.Value, out int cons);
        if (!bool.TryParse(effect.Attributes["selftar"]?.Value, out bool selfTar))
        {
            Debug.LogWarning("No value 'selfTar' found for this heal node. Using default value of 'true'.");
            selfTar = true;
        }

        if (!usingConst)
        {
            XmlNodeList subnodes = effect.ChildNodes;
            if (subnodes == null || subnodes.Count != 1)
            {
               throw new Exception("CardFactory::ParseVarCompareOp(" + effect.Name + ") -- Wrong number of subeffects. Wanted 1.");
            }

            return new Heal(ParseSingleEffect(subnodes[0]), selfTar);
        }
        else
        {
            return new Heal(cons, selfTar);
        }
    }

    // <maniphand val="11" tar="left" op="discard">
    // <effectA/> (if you don't have a const)
    // </maniphand>
    // tar can be "left", "right", "leftmost", "rightmost", "random"
    // op can be "discard" or "banish"
    // val doesn't matter with discard.
    private static IEffect ParseManipHand(XmlNode effect)
    {
        XmlNodeList subnodes = effect.ChildNodes;
        bool usingEffect = (subnodes != null && subnodes.Count == 1);
        ManipulateHand.ManipulateHandEffectEnum discardOrBanish;
        ManipulateHand.ManipulateHandTargetEnum target;
        
        string tar = effect.Attributes["tar"]?.Value;
        if (tar == null)
            throw new Exception("CardFactory::ParseManipHand(" + effect.Name + ") -- No 'tar' attribute found.");
        switch (tar)
        {
            case "left":
                target = ManipulateHand.ManipulateHandTargetEnum.leftCard;
                break;
            case "right":
                target = ManipulateHand.ManipulateHandTargetEnum.rightCard;
                break;
            case "leftmost":
                target = ManipulateHand.ManipulateHandTargetEnum.leftMostCard;
                break;
            case "rightmost":
                target = ManipulateHand.ManipulateHandTargetEnum.rightMostCard;
                break;
            case "random":
                target = ManipulateHand.ManipulateHandTargetEnum.random;
                break;
            default:
                throw new Exception("CardFactory::ParseManipHand(" + effect.Name + ") -- Unknown value for tar called " + tar);
        }
        string op = effect.Attributes["op"]?.Value;
        if (op == null)
            throw new Exception("CardFactory::ParseManipHand(" + effect.Name + ") -- No 'op' attribute found.");

        switch (op)
        {
            case "discard":
                discardOrBanish = ManipulateHand.ManipulateHandEffectEnum.discard;
                break;
            case "banish":
                discardOrBanish = ManipulateHand.ManipulateHandEffectEnum.banish;
                break;
            default:
                throw new Exception("CardFactory::ParseManipHand(" + effect.Name + ") -- Unknown value for op called " + op);
        }
        if (usingEffect)
        {
            return new ManipulateHand(ParseSingleEffect(subnodes[0]), target, discardOrBanish);
        }
        else
        {
            
            if (!int.TryParse(effect.Attributes["val"]?.Value, out int cons))
                cons = 1;
            return new ManipulateHand(cons, target, discardOrBanish);
        }
    }

    #endregion


    /// <summary>
    /// Gets the card with the given card name. Returns null if that card does not exist
    /// </summary>
    /// <param name="cardName">Name of the card</param>
    /// <returns></returns>
    public static Card GetCard(string cardName)
    {
        if (CardDictionary.ContainsKey(cardName.ToLower()))
        {
            return CardDictionary[cardName.ToLower()];
        }
        return null;
    }

    /// <summary>
    /// Gets the card with the given ID. Returns null if that card does not exist
    /// </summary>
    /// <param name="id">ID of the card</param>
    /// <returns></returns>
    public static Card GetCardByID(int id)
    {
        foreach (var card in CardDictionary.Values)
        {
            if (card.CardInfo.ID == id)
            {
                return card;
            }
        }
        return null;
    }
}

