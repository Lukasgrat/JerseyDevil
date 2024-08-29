using UnityEngine;
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
    public float MAXHEALTH = 100;
    public float health = 100;
 
    public float stamina= 100;
    public Slider healthSlider;
    public Slider staminaSlider;
    public bool isSprinting  = false;


    bool isZooming = false;


    [Header("Armor Related Attributes")]
    public float MAXARMOR = 50;
    float armor;
    public Slider armorSlider;
    public float armorRecoveryRate;
    public float armorStartRecoverTime;
    float armorRecoveryTimer;

    [Header("Death Related Atttributes")]
    public float deathTimer = 2f;
    public GameObject deathText;
    float currentDeathTimer;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        armor = MAXARMOR;
        armorRecoveryTimer = 0f;
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
        ArmorLogic();
    }


    private void ArmorLogic() 
    {
        armorRecoveryTimer = Mathf.Max(armorRecoveryTimer - Time.deltaTime, 0);
        if (armorRecoveryTimer == 0) 
        {
            armor = Mathf.Min(MAXARMOR, armor + armorRecoveryRate * Time.deltaTime);
            armorSlider.value = armor / MAXARMOR;
        }
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
        if (armor == 0)
        {
            health -= damage;
            health = Mathf.Clamp(health, 0, 100);
            healthSlider.value = health / MAXHEALTH;
            if (health <= 0)
            {
                health = 0;

            }
            armorRecoveryTimer = armorStartRecoverTime * 2;
        }
        else 
        {

            armor = Mathf.Max(0, armor - damage);
            if (armor == 0)
            {
                armorRecoveryTimer = armorStartRecoverTime * 2;
            }
            else 
            {
                armorRecoveryTimer = armorStartRecoverTime;
            }
        }
        armorSlider.value = armor / MAXARMOR;
    }

    public void sendZoomingSignal(bool curZoom) 
    {
        isZooming = curZoom;
    }
}
