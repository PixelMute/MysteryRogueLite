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
    public static void LoadCards()
    {
        var fileName = Application.dataPath + "/Resources/cardinfo.csv";
        try
        {
            using (var fileReader = new StreamReader(fileName))
            {
                CreateCardDictionary(fileReader);
            }
        }
        catch (Exception e)
        {
            throw new Exception("Unable to open card info file.", e);
        }
    }

    private static void CreateCardDictionary(StreamReader file)
    {
        CardDictionary = new Dictionary<string, Card>();
        var header = file.ReadLine();
        while (!file.EndOfStream)
        {
            var line = file.ReadLine().Split(',');
            var cardInfo = new CardInfo()
            {
                ID = int.Parse(line[0]),
                SpiritCost = int.Parse(line[1]),
                EnergyCost = int.Parse(line[2]),
                Name = line[3].Trim(),
                Description = line[4].Trim(),
            };
            var effectString = line[5];
            var effectList = EffectFactory.GetListOfEffectsFromString(effectString.Trim());
            var minRange = int.Parse(line[6]);
            var maxRange = int.Parse(line[7]);

            var conditions = RangeFactory.GetPlayConditionsFromString(line[8].Trim());
            var range = new Range(conditions, minRange, maxRange);
            var card = new Card(cardInfo, range, effectList);
            CardDictionary.Add(cardInfo.Name.ToLower(), card);
        }
    }

    public static Card GetCard(string cardName)
    {
        if (CardDictionary.ContainsKey(cardName.ToLower()))
        {
            return CardDictionary[cardName.ToLower()];
        }
        return null;
    }

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

