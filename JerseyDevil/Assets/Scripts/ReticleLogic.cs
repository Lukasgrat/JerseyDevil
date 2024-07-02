using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReticleLogic : MonoBehaviour
{
    public RectTransform topReticle;
    public RectTransform bottomReticle;
    public RectTransform leftReticle;
    public RectTransform rightReticle;
    float startingPos;
    public float maxOutPos;
    float maxReticleTimer = 0;
    float currentReticleTimer;
    // Start is called before the first frame update
    void Start()
    {
        startingPos = topReticle.localPosition.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (maxReticleTimer == 0) return;
        if (currentReticleTimer < maxReticleTimer)
        {
            currentReticleTimer += Time.deltaTime;
            moveReticles();
        }
        else
        {
            moveReticles();
            currentReticleTimer = 0;
            maxReticleTimer = 0;
        }
    }

    void moveReticles() 
    {
        float percentageComplete = currentReticleTimer / maxReticleTimer;
        float movement = (-1 * Mathf.Abs(percentageComplete - .5f) + .5f) * 2 * (maxOutPos - startingPos) + startingPos;
        topReticle.localPosition = new Vector2(0, movement);
        bottomReticle.localPosition = new Vector2(0, -movement);
        leftReticle.localPosition = new Vector2(-movement, 0);
        rightReticle.localPosition = new Vector2(movement, 0);
    }



    public void InitiateReticle(float time) 
    {
        maxReticleTimer = time;
    }
}
