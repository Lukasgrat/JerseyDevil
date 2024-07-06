using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHead : MonoBehaviour
{
    public EnemyImproved enemy;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (enemy.currentState == EnemyImproved.FSMStates.dead) 
        {
            GetComponent<CapsuleCollider>().enabled = false;
        }
    }
}
