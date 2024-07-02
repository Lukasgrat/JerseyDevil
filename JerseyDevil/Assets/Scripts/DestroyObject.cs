using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyObject : MonoBehaviour
{
    public float detroyDelay;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, detroyDelay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
