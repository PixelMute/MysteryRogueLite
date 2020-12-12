using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private bool paused = false;
    public GameObject PauseCanvas;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!paused)
            {
                PauseGame();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    public void QuitGame()
    {
        SaveGameSystem.ExitGame();
    }

    public void MainMenu()
    {
        ResumeGame();
        SaveGameSystem.instance?.SaveGame();
        SceneManager.LoadScene(SceneConstants.MainMenu);
    }

    public void Resume()
    {
        if (paused)
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0;
        paused = true;
        PauseCanvas.SetActive(true);
    }

    private void ResumeGame()
    {
        Time.timeScale = 1;
        paused = false;
        PauseCanvas.SetActive(false);
    }
}
