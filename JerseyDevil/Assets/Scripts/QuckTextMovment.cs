using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuckTextMovment : MonoBehaviour
{
    TMP_Text text;
    GameObject player;
    public DuckGrenade DG;
    int sendSignal;
    float timer = 0;
    Vector3 originalLocalPositon;
    Vector3 originalScale;
    Color originalColor;
    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
        player = FindAnyObjectByType<PlayerShooting>().gameObject;
        sendSignal = DG.sendSignal;
        Debug.Log($"Send Signal: {sendSignal}");
        timer = sendSignal;
        originalColor = text.color;
        originalLocalPositon = transform.localPosition;
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        timer = Mathf.Max(0, timer - Time.deltaTime);
        transform.localScale = originalScale * (((1 - (timer/sendSignal)) * 2) + 1);
        Color newColor = text.color;
        newColor.a = 1 - timer / sendSignal;
        text.color = newColor;
        transform.Translate(0f, Time.deltaTime * 3, 0f);
        if (timer == 0) 
        {
            timer = sendSignal;
            transform.localScale = originalScale;
            transform.localPosition = originalLocalPositon;
            text.color = originalColor;
        }
        transform.rotation = Quaternion.identity;
        Vector3 directionToTarget = (player.transform.position - transform.position).normalized;

        directionToTarget.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = lookRotation;
        transform.Rotate(0, 180, 0);
    }
}
