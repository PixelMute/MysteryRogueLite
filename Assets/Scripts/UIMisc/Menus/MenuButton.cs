using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

class MenuButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private List<Image> Images;
    private Button button;

    public void Awake()
    {
        button = GetComponentInChildren<Button>();
        Images = GetComponentsInChildren<Image>().ToList();
    }


    private void Highlight()
    {
        foreach (var image in Images)
        {
            image.color = new Color(1, 1, 1);
        }
    }

    private void UnHighlight()
    {
        foreach (var image in Images)
        {
            image.color = new Color(200f / 255f, 200f / 255f, 200f / 255f);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        button?.onClick.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Highlight();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UnHighlight();
    }
}

