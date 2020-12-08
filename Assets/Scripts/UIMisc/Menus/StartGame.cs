using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void RunGame()
    {
        SceneManager.LoadScene(SceneConstants.PlayGame);
    }
}

