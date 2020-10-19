using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreen : MonoBehaviour
{
    public static bool PlayerDied = false;
    public TextMeshProUGUI TitleText;


    public void PlayAgainButton()
    {
        SceneManager.LoadScene(SceneConstants.PlayGame);
    }

    public void QuitButton()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public static void PlayerDeath()
    {
        PlayerDied = true;
        SceneManager.LoadScene(SceneConstants.GameOver);
    }

    public static void PlayerWon()
    {
        PlayerDied = false;
        SceneManager.LoadScene(SceneConstants.GameOver);
    }


    // Start is called before the first frame update
    void Start()
    {
        if (PlayerDied)
        {
            TitleText.text = "You died...";
        }
        else
        {
            TitleText.text = "You won!";
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
