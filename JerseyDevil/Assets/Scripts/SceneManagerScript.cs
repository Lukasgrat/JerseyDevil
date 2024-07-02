using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    public GameObject eventHandler;
    public void LoadScene(string sceneName)
    {
        SceneManager.UnloadSceneAsync("IntroScene");
        eventHandler.SetActive(false);
        SceneManager.LoadScene(sceneName);
        
    }

    public void LoadScene(int sceneIndex)
    {
        SceneManager.UnloadSceneAsync("IntroScene");
        eventHandler.SetActive(false);
        SceneManager.LoadScene(sceneIndex);
        
    }

    public void QuitGame()
    {
        Application.Quit();
    }


}

