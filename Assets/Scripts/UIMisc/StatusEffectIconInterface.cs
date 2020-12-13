using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

// Attached to the gameobject representing an icon of a status effect. Interfaces to the Unity display.
// Doesn't actually hold data.
public class StatusEffectIconInterface : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private UnityEngine.UI.Image statusIcon;
    private TextMeshProUGUI numberText;
    private BattleManager.StatusEffectEnum setType;

    [SerializeField] private Image tooltipBackground;
    [SerializeField] private TextMeshProUGUI tooltipText;

    /*private void Start()
    {
        string[] names = new string[] { "StatusEffectArtwork", "StatusEffectIconTextObject"};
        Type[] types = new Type[] { typeof(UnityEngine.UI.Image), typeof(TextMeshProUGUI)};
        Component[] foundComponents = BattleManager.RecursiveVariableAssign(gameObject, names, types);
        statusIcon = foundComponents[0] as Image;
        numberText = foundComponents[1] as TextMeshProUGUI;
    }*/

    public void SetStatusEffect(BattleManager.StatusEffectEnum type)
    {
        Debug.Log("Setting status effect " + type + ", " + (int)type);

        // Set up vars
        string[] names = new string[] { "StatusEffectArtwork", "StatusEffectIconTextObject" };
        Type[] types = new Type[] { typeof(UnityEngine.UI.Image), typeof(TextMeshProUGUI)};
        Component[] foundComponents = BattleManager.RecursiveVariableAssign(gameObject, names, types);
        statusIcon = foundComponents[0] as Image;
        numberText = foundComponents[1] as TextMeshProUGUI;


        Sprite targetImage = BattleManager.instance.statusEffectReference[(int)type];
        statusIcon.sprite = targetImage;
        setType = type;

        tooltipBackground.gameObject.SetActive(false);
        switch (setType)
        {
            case BattleManager.StatusEffectEnum.defense:
                tooltipText.SetText("Defense takes damage before your health does.");
                break;
            case BattleManager.StatusEffectEnum.insight:
                tooltipText.SetText("Each point of insight boosts your next instance of damage by 100%.");
                break;
            case BattleManager.StatusEffectEnum.momentum:
                tooltipText.SetText("Momentum X boosts each instance of damage done by your next card by X.");
                break;
            case BattleManager.StatusEffectEnum.spiritLoss:
                tooltipText.SetText("You're low on spirit. Go down floors faster to recover.\nEach stage slows your card draw, and lowers your redraw amount. If you're out of spirit, you'll start losing health instead.");
                break;
        }
    }

    public void SetNumber(int input)
    {
        numberText.SetText(input.ToString());
    }

    public BattleManager.StatusEffectEnum GetStatusEffectType()
    {
        return setType;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        tooltipBackground.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipBackground.gameObject.SetActive(false);
    }
}
