using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuBehavior : MonoBehaviour
{
    public static bool isGamePause = false;
    public GameObject pauseMenu;
    public GameObject objective;

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isGamePause)
            {
                ResumeGame();
            }
            else 
            {
                PauseGame();
            }
        }
    }

    void PauseGame()
    {
        isGamePause = true;
        Time.timeScale = 0f;
        pauseMenu.SetActive(true);
        objective.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        GameObject enemiesLeft = GameObject.FindGameObjectWithTag("EnemyCount");
        if (enemiesLeft != null)
        {
            enemiesLeft.GetComponent<CanvasGroup>().alpha = 0;
        }
    }

    public void ResumeGame()
    {
        isGamePause = false;
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);
        objective.SetActive(true);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        GameObject enemiesLeft = GameObject.FindGameObjectWithTag("EnemyCount");
        if (enemiesLeft != null)
        {
            enemiesLeft.GetComponent<CanvasGroup>().alpha = 1;
        }
    }
}
