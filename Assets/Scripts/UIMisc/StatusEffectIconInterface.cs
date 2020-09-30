using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

// Attached to the gameobject representing an icon of a status effect. Interfaces to the Unity display.
// Doesn't actually hold data.
public class StatusEffectIconInterface : MonoBehaviour
{
    private UnityEngine.UI.Image statusIcon;
    private TextMeshProUGUI numberText;
    private BattleManager.StatusEffectEnum setType;

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
        Type[] types = new Type[] { typeof(UnityEngine.UI.Image), typeof(TextMeshProUGUI) };
        Component[] foundComponents = BattleManager.RecursiveVariableAssign(gameObject, names, types);
        statusIcon = foundComponents[0] as Image;
        numberText = foundComponents[1] as TextMeshProUGUI;

        Sprite targetImage = BattleManager.instance.statusEffectReference[(int)type];
        statusIcon.sprite = targetImage;
        setType = type;
    }

    public void SetNumber(int input)
    {
        numberText.SetText(input.ToString());
    }

    public BattleManager.StatusEffectEnum GetStatusEffectType()
    {
        return setType;
    }
}
