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
                case "playconditions":
                    playConditionList = ParsePlayConditions(subnode);
                    break;
                case "theme":
                    cardInfo.ThemeName = subnode.InnerText;
                    break;
            }
        }

        if (playConditionList == null || effectList == null)
        {
            throw new Exception("CardFactory::ParseCardXML() -- Card named " + cardInfo.Name + " does not have play conditions and/or effect list.");
        }

        // Now that we've done that, compile the info into an actual card.
        var range = new Range(playConditionList, minRange, maxRange);
        var card = new Card(cardInfo, range, effectList);

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
            res = BattleManager.StatusEffectEnum.defence;
        }
        return new DamageBasedOnStatus(res);
    }

    private static IEffect ParseApplyStatusEffect(XmlNode effect)
    {
        if (!Enum.TryParse(effect.Attributes["status"]?.Value, out BattleManager.StatusEffectEnum res))
        {
            Debug.LogWarning("No value 'status' found for this applystatuseffect node. Using default value.");
            res = BattleManager.StatusEffectEnum.defence;
        }

        if (!int.TryParse(effect.Attributes["power"]?.Value, out int power))
        {
            Debug.LogWarning("No value 'power' found for this applystatuseffect node. Using default value.");
            power = 1;
        }

        if (!bool.TryParse(effect.Attributes["self"]?.Value, out bool self))
        {
            Debug.LogWarning("No value 'self' found for this applystatuseffect node. Using default value.");
            self = true;
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

