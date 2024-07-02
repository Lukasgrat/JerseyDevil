using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetTimeText : MonoBehaviour
{
    public Text timeText;
    
    void Start()
    {
        timeText.GetComponent<Text>().text = PlayerPrefs.GetInt("DaysPlayed", 0) + " Days " + PlayerPrefs.GetInt("HoursPlayed", 0) + " Hours " + PlayerPrefs.GetInt("MinsPlayed", 0) + " Mins";
    }

}
