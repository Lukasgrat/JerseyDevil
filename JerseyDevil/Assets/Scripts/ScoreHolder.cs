using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreHolder : MonoBehaviour
{

    public static ScoreHolder instance;
    TMP_Text text;
    float maxTime = 3f;
    float timer = 0;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        text = GetComponent<TMP_Text>(); 
    }

    // Update is called once per frame
    void Update()
    {
        timer = Mathf.Max(timer - Time.deltaTime, 0);
        if (timer == 0) 
        {
            text.text = "";
        }
    }

    public void ReceiveScore(string score) 
    {
        text.text = "Recent Score: " + score;
        timer = maxTime;
    }
}
