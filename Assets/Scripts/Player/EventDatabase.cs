using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class holds the results of every event done, and executes the choice made.
namespace Roguelike
{
    public class EventDatabase
    {
        public enum EventEnum { startingBoon };
        public RogueEvent[] allEvents;
        public static EventDatabase instance;

        public EventDatabase()
        {
            if (instance == null)
            {
                instance = this;
                BuildEventArray();
            }
            else if (instance != this)
            {
                Debug.LogError("Error in EventDatabase::Initialize. Trying to make two EventDatabases.");
            }
        }

        // Ideally, this would load from a file.
        private void BuildEventArray()
        {
            allEvents = new RogueEvent[1];
            allEvents[0] = new StartingBoonEvent();
        }

        public static void ResolveEvent(EventEnum evChoice, int choice)
        {
            switch (evChoice)
            {
                case EventEnum.startingBoon:
                    EventDatabase.instance.allEvents[0].ResolveEvent(choice);
                    break;
                default:
                    Debug.LogError("Error in EventDatabase::ResolveEvent. Event " + evChoice.ToString() + " is unknown.");
                    break;
            }
        }

        public static RogueEvent GetEvent(EventEnum evChoice)
        {
            switch (evChoice)
            {
                case EventEnum.startingBoon:
                    return EventDatabase.instance.allEvents[0];
                default:
                    throw new Exception("Error in EventDatabase::ResolveEvent. Event " + evChoice.ToString() + " is unknown.");
            }
        }
        
    }

    public abstract class RogueEvent
    {
        public int NumChoices { get; protected set; }
        public string[] ChoiceDescriptions { get; protected set; }
        public string Description { get; protected set; }
        public abstract void ResolveEvent(int choice);
    }

    public class StartingBoonEvent : RogueEvent
    {
        public StartingBoonEvent()
        {
            NumChoices = 7;
            ChoiceDescriptions = new string[NumChoices];
            Description = "The memories of fire, blood, and destruction are seared into your mind. The pain is vivid, even if the images are not. But for now, you know " +
                "that in this strange place, you must rely on your skills as a...";
            ChoiceDescriptions[0] = "Wanderer\nGain 3 random cards.";
            ChoiceDescriptions[1] = "Lumberjack\nGain +30 max hp.";
            ChoiceDescriptions[2] = "Merchant\nGain 50 coins.";
            ChoiceDescriptions[3] = "Collector\nGain 2 Hanafuda cards.";
            ChoiceDescriptions[4] = "Curator\nGain 2 random Draconic cards.";
            ChoiceDescriptions[5] = "Highwayman\nGain 2 random Impundulu cards.";
            ChoiceDescriptions[6] = "Miner\nGain 2 random Al'Miraj cards.";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="choice">
        /// 0: Wanderer - Gain 3 random cards.
        /// 1: Lumberjack - +30 max hp.
        /// 2: Merchant - +45 coins.
        /// 3: Collector - 2 Hanafuda cards
        /// 4: Curator - 2 random draconic cards
        /// 5: Highwayman - 2 random impundulu cards
        /// 6: Miner - 2 random almiraj cards</param>
        public override void ResolveEvent(int choice)
        {
            switch (choice)
            {
                case 0:
                    Card card1 = CardFactory.GetRandomCard();
                    Card card2 = CardFactory.GetRandomCard();
                    Card card3 = CardFactory.GetRandomCard();
                    BattleManager.player.GainCard(card1, false);
                    BattleManager.player.GainCard(card2, false);
                    BattleManager.player.GainCard(card3, false);
                    BattleManager.player.puim.ShowAlert("Gained " + card1.CardInfo.Name + ", " + card2.CardInfo.Name + ", and " + card3.CardInfo.Name);
                    break;
                case 1:
                    BattleManager.player.GainMaxHP(30);
                    break;
                case 2:
                    BattleManager.player.GainMoney(50);
                    break;
                case 3:
                    Card hanaFuda = CardFactory.GetCardTheme("hanafuda").GetRandomCardInTheme();
                    BattleManager.player.GainInventoryCard(hanaFuda);
                    Card hanaFuda2 = CardFactory.GetCardTheme("hanafuda").GetRandomCardInTheme();
                    BattleManager.player.GainInventoryCard(hanaFuda2);
                    BattleManager.player.puim.ShowAlert("Gained " + hanaFuda.CardInfo.Name + ", and " + hanaFuda2.CardInfo.Name);
                    break;
                case 4:
                    Card drac = CardFactory.GetCardTheme("draconic").GetRandomCardInTheme();
                    BattleManager.player.GainCard(drac, false);
                    Card drac2 = CardFactory.GetCardTheme("draconic").GetRandomCardInTheme();
                    BattleManager.player.GainCard(drac2, false);
                    BattleManager.player.puim.ShowAlert("Gained " + drac.CardInfo.Name + ", and " + drac2.CardInfo.Name);
                    break;
                case 5:
                    Card imp = CardFactory.GetCardTheme("impundulu").GetRandomCardInTheme();
                    BattleManager.player.GainCard(imp, false);
                    Card imp2 = CardFactory.GetCardTheme("impundulu").GetRandomCardInTheme();
                    BattleManager.player.GainCard(imp2, false);
                    BattleManager.player.puim.ShowAlert("Gained " + imp.CardInfo.Name + ", and " + imp2.CardInfo.Name);
                    break;
                case 6:
                    Card alm = CardFactory.GetCardTheme("almiraj").GetRandomCardInTheme();
                    BattleManager.player.GainCard(alm, false);
                    Card alm2 = CardFactory.GetCardTheme("almiraj").GetRandomCardInTheme();
                    BattleManager.player.GainCard(alm2, false);
                    BattleManager.player.puim.ShowAlert("Gained " + alm.CardInfo.Name + ", and " + alm2.CardInfo.Name);
                    break;
            }
        }
    }
}
