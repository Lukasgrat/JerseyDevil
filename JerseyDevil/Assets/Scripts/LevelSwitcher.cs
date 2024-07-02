using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSwitcher : MonoBehaviour
{
    public string level1Name = "Level1";
    public string level2Name = "Level2";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Assuming player is tagged with "Player"
        {
            // Unload current level
            SceneManager.UnloadSceneAsync(level1Name);
            SceneManager.UnloadSceneAsync("World");
            SceneManager.UnloadSceneAsync("IntroScene");
            
            // Load next level
            SceneManager.LoadScene(level2Name);

        }
    }
}
