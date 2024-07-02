using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyCounter : MonoBehaviour
{

    int enemyCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddEnemy() 
    {
        enemyCount++;
        this.GetComponent<TMP_Text>().text = "Enemies Left: " + enemyCount;
    }

    public void RemoveEnemy()
    {
        enemyCount--;
        this.GetComponent<TMP_Text>().text = "Enemies Left: " + enemyCount;
    }
}
