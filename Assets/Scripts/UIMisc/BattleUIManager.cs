using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// A collection of methods that UI buttons call
public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void ToggleMassCardView()
    {
        BattleManager.player.puim.ToggleShowMassCardView();
    }

    // Sets a toggle for which sets of cards to view.
    // 0 - Draw, 1 - Discard, 2 - Hand, 3 - Banish
    public void SetMassCardViewButtonToggle(int index)
    {
        BattleManager.player.puim.ToggleMassCardViewOption(index);
    }
}
