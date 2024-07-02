using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    int enemyCount = 0;
    int aliveEnemies;

    // Start is called before the first frame update
    void Start()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        aliveEnemies = enemyCount;
    }

    // Update is called once per frame
    void Update()
    {
        if (aliveEnemies <= 0)
        {
            FindAnyObjectByType<StandardLevelManager>().EnemiesDead();
        }
    }

    public void enemyDied()
    {
        aliveEnemies--;
    }
}
