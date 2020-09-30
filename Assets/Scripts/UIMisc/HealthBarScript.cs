using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{

    public Image sliderImage;
    public Gradient gradient;

    private TextMeshProUGUI healthTextValue;
    private void Awake()
    {
        if (sliderImage == null)
        {
            Image[] list = gameObject.GetComponentsInChildren<Image>();
            foreach (Image x in list)
            {
                if (x.gameObject.name == "HealthBarFill")
                {
                    sliderImage = x;
                    break;
                }
            }
        }

        if (healthTextValue == null)
            healthTextValue = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateHealthDisplay(float currentVal, int maxVal)
    {
        float percentage = currentVal / maxVal;
        sliderImage.fillAmount = percentage;
        healthTextValue.text = currentVal + "";
        sliderImage.color = gradient.Evaluate(percentage);
        healthTextValue.color = gradient.Evaluate(percentage);
    }
}
