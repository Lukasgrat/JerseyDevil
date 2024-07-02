using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeOil : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, Time.deltaTime * 60, 0);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerController pc)) 
        {
            pc.TakeDamage(-20);
            Destroy(this.gameObject);
        }
    }
}
