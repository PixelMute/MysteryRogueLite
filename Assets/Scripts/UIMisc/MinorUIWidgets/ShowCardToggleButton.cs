using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A small script placed on the toggle buttons for showing various sets of cards
public class ShowCardToggleButton : MonoBehaviour
{
    public int buttonIndex;

    public void SetMassCardViewToggle()
    {
        BattleUIManager.instance.SetMassCardViewButtonToggle(buttonIndex);
    }
}
