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

    public void GetCardRewardButtonClicked()
    {
        BattleManager.player.GetCardReward();
    }

    public void ToggleCardDrawerPositionButtonClicked()
    {
        BattleManager.player.puim.ToggleCardDrawerPosition();
    }

    public void SkipCardRewardButtonClicked()
    {
        BattleManager.player.puim.LeaveCardRewardScreen(false);
    }

    // Sets a toggle for which sets of cards to view.
    // 0 - Draw, 1 - Discard, 2 - Hand, 3 - Banish
    public void SetMassCardViewButtonToggle(int index)
    {
        BattleManager.player.puim.ToggleMassCardViewOption(index);
    }

    /// <summary>
    /// Close out of windows
    /// </summary>
    public void BackShadingClicked()
    {
        BattleManager.player.puim.CloseOutOfWindows();
    }
}
