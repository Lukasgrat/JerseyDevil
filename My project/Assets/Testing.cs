using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    // Start is called before the first frame update
    Camera testing;
    float lmao;
    Player player;
    void Start()
    {
        testing = Camera.main;
        lmao = Camera.main.orthographicSize;
        player = FindObjectOfType<Player>();
        Debug.Log(player.gameObject.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
