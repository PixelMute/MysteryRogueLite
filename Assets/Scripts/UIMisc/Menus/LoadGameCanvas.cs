using System.Collections.Generic;
using UnityEngine;

class LoadGameCanvas : MonoBehaviour
{
    public bool RegularGames = true;
    public GameObject LoadGameScroll;
    private SaveGameSystem SaveGameSystem;

    public void Awake()
    {
        SaveGameSystem = new SaveGameSystem();
        var locations = new List<Vector3>() { new Vector3(-225, 0, 0), new Vector3(0, 0, 0), new Vector3(225, 0, 0) };
        for (int i = 0; i < 3; i++)
        {
            var scroll = Instantiate(LoadGameScroll, transform);
            scroll.transform.localPosition = locations[i];
            var info = SaveGameSystem.GetMetaData(i, RegularGames);
            scroll.GetComponent<LoadGameScroll>().SetInfo(info, RegularGames ? i : i + 3);
        }
    }
}

