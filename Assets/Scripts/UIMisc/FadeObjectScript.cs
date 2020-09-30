using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UIElements;

// Causes attached object to fade out over time 
public class FadeObjectScript : MonoBehaviour
{

    private float opacityValue;
    private float speedMultiplier;
    private TextMeshProUGUI tmp;
    private CanvasGroup cg;

    public enum FadeType { text, canvas};
    public FadeType type;

    void Start()
    {
        if (type == FadeType.text)
            tmp = GetComponent<TextMeshProUGUI>();
        else if (type == FadeType.canvas)
            cg = GetComponent<CanvasGroup>();
    }

    // Update is only called if we're active.
    void Update()
    {
        if (opacityValue >= 0)
        {
            switch (type)
            {
                case FadeType.text:
                    tmp.alpha = opacityValue;
                    break;
                case FadeType.canvas:
                    cg.alpha = opacityValue;
                    break;

            }

            opacityValue -= speedMultiplier * Time.deltaTime;

            if (opacityValue <= 0)
                gameObject.SetActive(false);
        }
        
    }

    public void StartFadeCycle(string text, float buffer, float speedMult)
    {
        if (type == FadeType.text)
            tmp.text = text;
        StartFadeCycle(buffer, speedMult);
    }

    public void StartFadeCycle(float buffer, float speedMult)
    {
        speedMultiplier = speedMult;
        opacityValue = 1 + buffer;
        gameObject.SetActive(true);
    }

    // Interupts the fade cycle wherever it may be
    public void StopFade(float forcedOpacity)
    {
        opacityValue = -1;
        switch (type)
        {
            case FadeType.text:
                tmp.alpha = forcedOpacity;
                break;
            case FadeType.canvas:
                cg.alpha = forcedOpacity;
                break;

        }
    }
}
