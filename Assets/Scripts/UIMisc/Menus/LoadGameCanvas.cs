using System.Collections.Generic;
using UnityEngine;

class LoadGameCanvas : MonoBehaviour
{
    public bool RegularGames = true;
    public GameObject LoadGameScroll;
    private SaveGameSystem SaveGameSystem;
    public GameObject Play;
    public GameObject Erase;
    public GameObject MainCanvas;
    private LoadGameScroll[] scrolls = new LoadGameScroll[3];
    private int clickedScroll = -1;

    public void Awake()
    {
        SaveGameSystem = new SaveGameSystem();
        var locations = new List<Vector3>() { new Vector3(-225, 0, 0), new Vector3(0, 0, 0), new Vector3(225, 0, 0) };
        for (int i = 0; i < 3; i++)
        {
            var scroll = Instantiate(LoadGameScroll, transform);
            scroll.transform.localPosition = locations[i];
            var info = SaveGameSystem.GetMetaData(i, RegularGames);
            var loadScroll = scroll.GetComponent<LoadGameScroll>();

            loadScroll.SetInfo(info, RegularGames ? i : i + 3);
            scrolls[i] = loadScroll;
        }
    }

    public void Clicked(int index)
    {
        for (int i = 0; i < 3; i++)
        {
            if (i != index)
            {
                scrolls[i].gameObject.SetActive(false);
            }
        }
        Play.SetActive(true);
        Erase.SetActive(true);
        clickedScroll = index;
    }

    public void PlayButton()
    {
        if (clickedScroll != -1)
        {
            StartCoroutine(SaveGameSystem.StartGameScene(clickedScroll));
            StartLoadingScreen();
        }
    }

    public void EraseButton()
    {
        if (clickedScroll != -1)
        {
            SaveGameSystem.DeleteGame(clickedScroll);
            for (int i = 0; i < 3; i++)
            {
                var info = SaveGameSystem.GetMetaData(i, RegularGames);
                scrolls[i].SetInfo(info, i);
                scrolls[i].gameObject.SetActive(true);
            }
            Play.SetActive(false);
            Erase.SetActive(false);
            clickedScroll = -1;
        }
    }

    public void Back()
    {
        if (clickedScroll == -1)
        {
            MainCanvas.SetActive(true);
            gameObject.SetActive(false);
        }
        else
        {
            clickedScroll = -1;
            Play.SetActive(false);
            Erase.SetActive(false);
            for (int i = 0; i < 3; i++)
            {
                scrolls[i].gameObject.SetActive(true);
            }
        }
    }

    private void StartLoadingScreen()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }
        for (int i = 0; i < 3; i++)
        {
            scrolls[i].gameObject.SetActive(false);
        }
    }
}

