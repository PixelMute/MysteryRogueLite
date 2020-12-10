using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

class LoadGameScroll : MonoBehaviour, IPointerClickHandler
{
    public TextMeshProUGUI ScrollText;
    public GameObject GameInfo;
    public TextMeshProUGUI FloorNumber;
    public GameMetaData Data;
    public GameObject Parent;

    void Awake()
    {
        Parent = transform.parent.gameObject;
    }

    public void SetInfo(GameMetaData data)
    {
        Data = data;
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
        if (Data == null)
        {
            //Start new game
            StartCoroutine(SaveGameSystem.StartGameScene(Data));
            StartLoadingScreen();
        }
    }

    private void StartLoadingScreen()
    {
        foreach (Transform child in Parent.transform)
        {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
    }
}

