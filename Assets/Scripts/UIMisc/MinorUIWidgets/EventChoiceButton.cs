using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Roguelike;

public class EventChoiceButton : MonoBehaviour
{
    public EventDatabase.EventEnum EventEnum { get; private set; }
    public int EventChoice { get; private set; }
    public TextMeshProUGUI buttonText;

    public void SetValues(EventDatabase.EventEnum evEnum, int choice)
    {
        EventEnum = evEnum;
        EventChoice = choice;
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }

    public void RegisterClick()
    {
        BattleManager.player.puim.ResolveEvent(EventEnum, EventChoice);
    }
}
