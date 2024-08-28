using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreSender : MonoBehaviour
{
    public string score;

    public void SendScore() 
    {
        ScoreHolder.instance.ReceiveScore(score);
    }
}
