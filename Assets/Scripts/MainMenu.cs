using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update

    public void QuitGame()
    {
        Debug.Log("Exited Game");
        Application.Quit();
    }

    public void PlayTDM()
    {
        SceneManager.LoadScene(2);
        Time.timeScale = 1;
    }

    public void PlaySurvival()
    {
        SceneManager.LoadScene(1);
        Time.timeScale = 1;
    }

    public void PlayChaos()
    {
        SceneManager.LoadScene(3);
        Time.timeScale = 1;
    }

    public void OpenMenu()
    {
        SceneManager.LoadScene(0);
        Time.timeScale = 1;
    }
}
