using UnityEngine;

public class ObjectDestroyer : MonoBehaviour
{
    // Reference to the GameObject to be destroyed
    public GameObject objectToDestroy;

    // Update is called once per frame
    void Update()
    {
        // Find all GameObjects with the tag "Enemy"
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // Check if there are no enemies remaining
        if (enemies.Length == 0)
        {
            // Destroy the object
            Destroy(objectToDestroy);
        }
    }
}
