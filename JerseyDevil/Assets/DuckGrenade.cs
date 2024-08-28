using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DuckGrenade : MonoBehaviour
{
    float timer = 0;
    AudioSource audioSource;
    public int sendSignal = 1;
    // Start is called before the first frame update
    void Start()
    {
        timer = sendSignal;
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        timer = Mathf.Max(0, timer - Time.deltaTime);
        if (timer == 0)
        {
            foreach (EnemyImproved enemy in GameObject.FindObjectsOfType<EnemyImproved>())
            {
                enemy.OnPlayerFire(this.transform.position);
            }
            timer = sendSignal;
        }
    }
}
