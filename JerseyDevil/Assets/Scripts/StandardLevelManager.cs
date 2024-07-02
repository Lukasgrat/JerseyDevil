using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StandardLevelManager : MonoBehaviour
{
    public GameObject deathUI;
    public GameObject levelWinUI;
    public CanvasGroup fadeUI;
    public float deathScreenTime = 2;
    public bool objectiveComplete = false;
    float curTime = 0;
    public PlayerController playerController;
    public GameObject gameCompleteUI;

    // Start is called before the first frame update
    void Start()
    {
        playerController = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.IsDead())
        {
            curTime = Mathf.Min(curTime + Time.deltaTime, deathScreenTime);
            if (curTime >= deathScreenTime)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                deathUI.SetActive(true);
                fadeUI.gameObject.SetActive(true);
                levelWinUI.SetActive(false);
                fadeUI.alpha = curTime / deathScreenTime;
            }
        }
        else if (objectiveComplete)
        {
            curTime = Mathf.Min(curTime + Time.deltaTime, deathScreenTime);

            // Game Won
            if (SceneManager.GetActiveScene().buildIndex == 5)
            {
                Time.timeScale = 0f;
                gameCompleteUI.SetActive(true);
                Invoke("LoadMainMenu", 10);
            }
            // Level won
            else
            {
                if (curTime >= deathScreenTime)
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
                else
                {
                    levelWinUI.SetActive(true);
                    fadeUI.gameObject.SetActive(true);
                    fadeUI.alpha = curTime / deathScreenTime;
                }
            }

            
        }
    }

    public void ExplosionSignal() 
    {
        objectiveComplete = true;
    }

    public void EnemiesDead()
    {
        objectiveComplete = true;
    }
}
