using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class LevelManager : MonoBehaviour
{
    public Text gameText;
    public static bool isGameOver = false;
    public AudioClip gameOverSFX;
    public AudioClip gameWonSFX;
    public string nextLevel;
    public int score = 0;
    
    void Start()
    {
        isGameOver = false;
        gameText.gameObject.SetActive(false);
    }


    public void LevelLost()
    {
        if(!isGameOver)
        {
            isGameOver = true;
            gameText.text = "GAME OVER!";
            gameText.gameObject.SetActive(true);

            Camera.main.GetComponent<AudioSource>().pitch = 1;
            AudioSource.PlayClipAtPoint(gameOverSFX, Camera.main.transform.position);

            Invoke("LoadCurrentLevel", 2);
        }
        
    }

    public void LevelBeat()
    {
        if(!isGameOver)
        {
            isGameOver = true;
            gameText.text = "YOU WIN!";
            gameText.gameObject.SetActive(true);

            Camera.main.GetComponent<AudioSource>().pitch = 2;
            AudioSource.PlayClipAtPoint(gameWonSFX, Camera.main.transform.position);

            if(!string.IsNullOrEmpty(nextLevel))
            {
                Invoke("LoadNextLevel", 2);
            }
        }
        
        
    }

    void LoadNextLevel() 
    {
        SceneManager.LoadScene(nextLevel);

    }

    void LoadCurrentLevel() 
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
