using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CardFactory
{
    private static Dictionary<string, Card> CardDictionary;

    /// <summary>
    /// Loads the card information from the csv file
    /// </summary>
    public static void LoadCards()
    {
        var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            LineBreakInQuotedFieldIsBadData = false,     //Set so we don't get bad data if there is line break in description
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,     //Trim excess white space
        };
        var fileName = Application.dataPath + "/Resources/cardinfo.csv";
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
            throw new Exception("Unable to open card info file.", e);
        }
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
            CardDictionary.Add(cardInfo.Name.ToLower(), card);
        }
    }

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

