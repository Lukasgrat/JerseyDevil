using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    Transform playerBody;
    float mouseSensitivity;
    // Start is called before the first frame update
    
    public float recoilDuration = 1;
    public int recoilAmount = 30;
    private float currentRecoilFrame = 0;
    public PlayerController playerController;

    float pitch = 0;
    void Start()
    {
        playerBody = transform.parent.transform;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        mouseSensitivity = PlayerPrefs.GetInt("mouseSensitivity", 500);
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float moveY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime * 1.3f;


        //yaw
        playerBody.Rotate(Vector3.up * moveX);


        //pitch
        pitch -= moveY;
        checkRecoil();
        pitch = Mathf.Clamp(pitch, -90f, 90f);
        transform.localRotation = Quaternion.Euler(pitch, 0, 0);

    }

    public void iniateRecoil(int recoilAmount = 30) 
    {
        this.recoilAmount = recoilAmount;
        currentRecoilFrame = recoilDuration;
    }


    void checkRecoil() 
    {
        if (currentRecoilFrame > 0)
        {
            pitch -= Mathf.Pow(currentRecoilFrame / recoilDuration, 4) * Time.deltaTime * recoilAmount;
            currentRecoilFrame -= Time.deltaTime;
        }
        else 
        {
            currentRecoilFrame = 0;
        }
    }

    /// <summary>
    /// Returns the closest gameobject in the given list of hits, returning null if no collisions are made
    /// </summary>
    public GameObject inSights(RaycastHit[] hits)
    {
        GameObject shortestGameObject = null;
        float shortestDistance = float.MaxValue;
        foreach (RaycastHit hit in hits)
        {
            if (hit.distance < shortestDistance)
            {
                shortestDistance = hit.distance;
                shortestGameObject = hit.collider.gameObject;
            }
        }
        return shortestGameObject;
    }
}
