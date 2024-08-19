using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TextureTilingController : MonoBehaviour
{

    public int tileX;
    public int tileY;


    // Use this for initialization
    void Start()
    {
        gameObject.GetComponent<Renderer>().material.mainTextureScale = new Vector2(tileX / transform.lossyScale.x, tileY / transform.lossyScale.y); 
    }

    // Update is called once per frame
    void Update()
    {

        gameObject.GetComponent<Renderer>().material.mainTextureScale = new Vector2(tileX / transform.lossyScale.x, tileY / transform.lossyScale.y);
    }
}