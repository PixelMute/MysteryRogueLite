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

    void Awake()
    {
        Parent = transform.parent.gameObject;
    }

    public void SetInfo(GameMetaData data, int gameSlot)
    {
        _gameSlot = gameSlot;
        if (data != null)
        {
            ScrollText.text = data.LastPlayed.ToString("MM-dd-yy HH:mm");
            ScrollText.fontSize = 34;
            GameInfo.SetActive(true);
            FloorNumber.text = data.FloorNumber.ToString();
        }
        else
        {
            ScrollText.text = "Start New Game";
            ScrollText.fontSize = 40;
            GameInfo.SetActive(false);
        }
    }

    public void Delete()
    {

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!disabled)
        {
            StartCoroutine(SaveGameSystem.StartGameScene(_gameSlot));
            StartLoadingScreen();
            disabled = true;
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
    }
}

