using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Player attributes")]
    CharacterController controller;
    Vector3 input, moveDirection;
    public float jumpHeight = 10;
    public float gravity = 9.81f;
    public float moveSpeed = 10;
    public float airControl = 10f;
    public float health = 100;
    public float stamina= 100;
    public Slider healthSlider;
    public Slider staminaSlider;
    public bool isSprinting  = false;


    bool isZooming = false;

    [Header("Death Related Atttributes")]
    public float deathTimer = 2f;
    public GameObject deathText;
    float currentDeathTimer;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Debug.Log(controller);
    }

    // Update is called once per frame
    void Update()
    {

        if (IsDead()) return;
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        input = (transform.right * moveHorizontal + transform.forward * moveVertical).normalized;
        input *= moveSpeed;


        if (Input.GetKey(KeyCode.LeftShift) && stamina > 10)
        {
            isSprinting = true;
        }
        if (Input.GetButton("Fire1") || Input.GetButton("Fire2") || stamina == 0 || (moveVertical == 0 && moveHorizontal == 0))
        {
            isSprinting = false;
        }


        if (isSprinting) 
        {
            stamina = Mathf.Max(stamina - Time.deltaTime * 20 , 0);
            input  = input * 5 / 3;
        }
        else
        {
            stamina = Mathf.Min(stamina + Time.deltaTime * 10, 100);
        }

        staminaSlider.value = stamina;

        if (isZooming) 
        {
            input /= 2;
        }
        if (controller.isGrounded && !isZooming)
        {
            moveDirection = input;
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = Mathf.Sqrt(2 * jumpHeight * gravity);
            }
            else
            {
                moveDirection.y = 0.0f;
            }
        }
        else
        {
            input.y = moveDirection.y;
            moveDirection = Vector3.Lerp(moveDirection, input, airControl * Time.deltaTime);
        }


        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }


    /// <summary>
    /// Returns if this player is dead
    /// </summary>
    /// <returns></returns>
    public bool IsDead()
    {
        return health <= 0;
    }



    public void TakeDamage(int damage)
    {
        health -= damage;
        health = Mathf.Clamp(health, 0, 100);
        healthSlider.value = health;
        if (health <= 0)
        {
            Debug.Log("They do be dead");
            health = 0;

        }
    }

    public void sendZoomingSignal(bool curZoom) 
    {
        isZooming = curZoom;
    }
}
