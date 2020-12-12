using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

class LoadGameScroll : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI ScrollText;
    public GameObject GameInfo;
    public TextMeshProUGUI FloorNumber;
    private GameObject Parent;
    private int _gameSlot;
    private bool disabled = false;
    private bool hasGameSlot = false;

    void Awake()
    {
        Parent = transform.parent.gameObject;
    }

    public void SetInfo(GameMetaData data, int gameSlot)
    {
        _gameSlot = gameSlot;
        if (data != null)
        {
            hasGameSlot = true;
            ScrollText.text = data.LastPlayed.ToString("MM-dd-yy HH:mm");
            ScrollText.fontSize = 34;
            GameInfo.SetActive(true);
            FloorNumber.text = data.FloorNumber.ToString();
        }
        else
        {
            SetStartNewGameText();
        }
    }

    private void SetStartNewGameText()
    {
        hasGameSlot = false;
        ScrollText.text = "Start New Game";
        ScrollText.fontSize = 40;
        GameInfo.SetActive(false);
    }

    public void Delete()
    {
        SaveGameSystem.DeleteGame(_gameSlot);
        SetStartNewGameText();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!disabled)
        {
            if (hasGameSlot)
            {
                var canvas = Parent.GetComponent<LoadGameCanvas>();
                canvas.Clicked(_gameSlot);
            }
            else
            {
                disabled = true;
                StartCoroutine(SaveGameSystem.StartGameScene(_gameSlot));
                StartLoadingScreen();
            }
        }
    }

    private void StartLoadingScreen()
    {
        foreach (Transform child in Parent.transform)
        {
            if (child != transform)
            {
                child.gameObject.SetActive(!child.gameObject.activeSelf);
            }
        }
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        var canvas = Parent.GetComponent<LoadGameCanvas>();
        canvas?.Play?.SetActive(false);
        canvas?.Erase?.SetActive(false);
    }

}

