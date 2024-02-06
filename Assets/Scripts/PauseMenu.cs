using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{

    public static bool gameIsPaused = false;
    public GameObject pauseMenuUI;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(gameIsPaused)
            {
                Resume();
            }

            else
            {
                Pause();
            }
        }
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Cursor.lockState = CursorLockMode.None; //allow mouse to freely interact with pause menu and main menu if main menu option is selected
        Time.timeScale = 0; //freezes game after pause menu is activated
        gameIsPaused = true;
    }

    void Resume()
    {
        pauseMenuUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked; //returns mouse to locked state for gameplay
        Time.timeScale = 1; //resumes game at normal time scale
        gameIsPaused = false;
    }

    public void ReloadScene()
    {
        Time.timeScale = 1; //resets time scale
        gameIsPaused = false;
        String currentScene = SceneManager.GetActiveScene().name; //takes name of active scene
        SceneManager.LoadScene(currentScene);//uses active scene name to reload scene
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1; //resets time scale
        gameIsPaused = false;
        SceneManager.LoadScene(0);
    }
}
