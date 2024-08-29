using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;


public enum Gunplay
{
    Reloading,
    ReloadStart,
    ReloadEnd,
    ReloadMiddle,
    Readied,
    Firing,
    Holster,
    Dead,
}

public interface IGUN 
{
    public void ShootingLogic();
    public void Initialize(MouseLook PS, TMP_Text ammotext);

    public void Holster();

    public void UnHolster();

    public bool CanHolster();

    public void UpdateAmmoText();

    public bool CanThrowDynamite();
}

public class PlayerShooting : MonoBehaviour
{
    public float coolDownDynamiteTime = 5f;
    public float throwingForce = 100f;
    float cooldownDynamiteTimer = 0f;
    public Gunplay curState;
    public TMP_Text ammoText;
    public GameObject dynamite;
    public Slider dynamiteCooldown;
    IGUN currentGun;
    int currentGunIndex = 0;
    public List<GameObject> availableGuns;
    public Camera layeredCamera;

    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject gun in availableGuns) 
        {
            gun.GetComponent<IGUN>().Initialize(this.GetComponent<MouseLook>(), ammoText);
            gun.SetActive(false);
        }
        availableGuns[currentGunIndex].gameObject.SetActive(true);
        currentGun = availableGuns[currentGunIndex].GetComponent<IGUN>();
        curState = Gunplay.Readied;
        if (ammoText == null)
        {
            throw new System.Exception("Ammo counter is not linked up");
        }
    }

    //Update is called once per frame
    void Update()
    {
        layeredCamera.fieldOfView = Camera.main.fieldOfView;
        currentGun.ShootingLogic();
        SightHandler();
        ThrowingHandler();
        ShootSwapHandler();
    }

    void ShootSwapHandler() 
    {
        if (!currentGun.CanHolster() || FindAnyObjectByType<PlayerController>().IsDead()) return;
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2)) 
        {
            bool newGun = false;
            if (Input.GetKeyDown(KeyCode.Alpha1) && currentGunIndex != 0) 
            {
                currentGunIndex = 0;
                newGun = true;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) && currentGunIndex != 1)
            {
                currentGunIndex = 1;
                newGun = true;
            }

            if (!newGun) return;


            foreach (GameObject gun in availableGuns)
            {
                gun.SetActive(false);
            }
            availableGuns[currentGunIndex].SetActive(true);
            currentGun.UnHolster();
            currentGun = availableGuns[currentGunIndex].GetComponent<IGUN>();
            currentGun.UpdateAmmoText();
            currentGun.Holster();
        }
    }

    void ThrowingHandler() 
    {
        if (FindAnyObjectByType<PlayerController>().IsDead() || dynamiteCooldown == null) { return; }
        if (cooldownDynamiteTimer > 0)
        {
            cooldownDynamiteTimer -= Time.deltaTime;
        }
        else if(currentGun.CanThrowDynamite() &&
            (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt) || Input.GetKeyUp(KeyCode.Q)))
        {
            GameObject newDynamite = Instantiate(dynamite, this.transform.position + transform.forward.normalized * .25f, this.transform.rotation);
            newDynamite.GetComponent<Rigidbody>().AddForce(
                Quaternion.AngleAxis(0, Vector3.left) * transform.forward * throwingForce * newDynamite.GetComponent<Rigidbody>().mass, ForceMode.Force);
            cooldownDynamiteTimer = coolDownDynamiteTime;
            newDynamite.GetComponent<Rigidbody>().AddRelativeTorque(Vector3.right * 20);
        }
        dynamiteCooldown.value = 1- (cooldownDynamiteTimer / coolDownDynamiteTime);

    }

    void SightHandler()
    {
        GameObject sightedObject = inSights(Physics.RaycastAll(transform.position, transform.forward, 600));
        if (sightedObject != null && sightedObject.TryGetComponent(out EnemyImproved target) && !target.isDead)
        {
            target.HealthDisplay();
        }
        else if (sightedObject != null && sightedObject.TryGetComponent(out EnemyHead head) && !head.enemy.isDead)
        {
            head.enemy.HealthDisplay();
        }
        else
        {
            GameObject.FindGameObjectWithTag("EnemyDisplayName").GetComponent<CanvasGroup>().alpha = 0;
            GameObject.FindGameObjectWithTag("EnemyHealthSlider").GetComponent<CanvasGroup>().alpha = 0;
        }
    }

    /// <summary>
    /// Returns the closest gameobject in the given list of hits, returning null if no collisions are made
    /// </summary>
    private GameObject inSights(RaycastHit[] hits)
    {
        GameObject shortestGameObject = null;
        float shortestDistance = float.MaxValue;
        foreach (RaycastHit hit in hits)
        {
            if (hit.distance < shortestDistance && !(hit.collider.gameObject.tag == "Gun"))
            {
                shortestDistance = hit.distance;
                shortestGameObject = hit.collider.gameObject;
            }
        }
        return shortestGameObject;
    }
}


