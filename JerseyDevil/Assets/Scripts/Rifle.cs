using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Rifle : MonoBehaviour, IGUN
{
    public AudioClip reloadingSFX;
    Animator gunAnimator;
    public AudioClip shootingSFX;
    AudioSource shootingSFXSource;
    public float reloadTime = 3f;
    public float fireTime = .75f;
    public float holsterTime = .834f;
    float coolDownTime = 0f;
    public Gunplay curState;
    public int maxAmmo = 5;
    public float SIGHTINGTIME = .5f;
    float sightingTimer = 0;
    Vector3 startLocalPos;
    public Vector3 desiredZoomPos;
    int curAmmo;
    TMP_Text ammoText;
    MouseLook playerHead;
    float startFOV;
    public float decreaseInFOV = 20f;
    bool isZooming = false;
    // Start is called before the first frame update
    void Start()
    {
        curAmmo = maxAmmo;
        gunAnimator = transform.GetChild(0).GetComponent<Animator>();
        shootingSFXSource = GetComponent<AudioSource>();
        startLocalPos = transform.localPosition;

        startFOV = Camera.main.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButton("Fire2") && !FindAnyObjectByType<PlayerController>().IsDead() && (curState == Gunplay.Readied 
            || (sightingTimer == SIGHTINGTIME && curState == Gunplay.Firing)))
        {
            sightingTimer = Mathf.Min(sightingTimer + Time.deltaTime, SIGHTINGTIME);
        }
        else 
        {
            sightingTimer = Mathf.Max(sightingTimer - Time.deltaTime, 0);
        }

        float curTime = sightingTimer / SIGHTINGTIME;
        this.transform.localPosition =  startLocalPos * (1 - curTime)  + desiredZoomPos * curTime;
        Camera.main.fieldOfView = startFOV - decreaseInFOV * curTime;
        this.transform.localRotation = Quaternion.Euler(new Vector3(0, -5 * (1 - curTime), 0));

        if ((sightingTimer <= 0))
        {
            isZooming = false;
        }
        else 
        {
            isZooming = true;
        }


        GameObject.FindAnyObjectByType<PlayerController>().sendZoomingSignal(isZooming);
    }

    public void Initialize(MouseLook PS, TMP_Text ammotext)
    {
        playerHead = PS;
        this.ammoText = ammotext;
    }
    private void StateHandler()
    {
        if (curState == Gunplay.Readied) 
        {
            transform.GetChild(0).localPosition  = Vector3.zero;
            transform.GetChild(0).localRotation = Quaternion.identity;
            return;
        }
        float timer = 0;
        if (curState == Gunplay.Reloading)
        {
            timer = reloadTime;
        }
        else if (curState == Gunplay.Firing)
        {
            timer = fireTime;
        }
        else if(curState == Gunplay.Holster) 
        {
            timer = holsterTime;
        }
        if (coolDownTime < timer)
        {
            coolDownTime += Time.deltaTime;
        }
        else
        {
            if (curState == Gunplay.Reloading)
            {
                this.curAmmo = this.maxAmmo;
                UpdateAmmoText();
            }
            coolDownTime = 0;
            curState = Gunplay.Readied;
            gunAnimator.SetInteger("animState", 0);
            gunAnimator.gameObject.transform.localPosition = Vector3.zero;
        }
    }

    public void ShootingLogic()
    {
        StateHandler();
        if (curState != Gunplay.Readied ||  FindAnyObjectByType<PlayerController>().IsDead())
        {
            return;
        }


        if (Input.GetButtonDown("Fire1") && curAmmo > 0)
        {
            gunAnimator.gameObject.transform.localEulerAngles = new Vector3(0, -5, 0);
            gunAnimator.SetInteger("animState", 1);
            curState = Gunplay.Firing;
            StartCoroutine(shootingEffects());
        }
        if (!isZooming && (Input.GetKeyDown(KeyCode.R) || (curAmmo == 0 && Input.GetButtonDown("Fire1"))))
        {
            gunAnimator.gameObject.transform.localEulerAngles = new Vector3(0, -5, 0);
            curState = Gunplay.Reloading;
            shootingSFXSource.clip = reloadingSFX;
            shootingSFXSource.Play();
            gunAnimator.SetInteger("animState", 2);
        }
    }

    public void UpdateAmmoText()
    {
        this.ammoText.text = curAmmo + " / " + maxAmmo;
    }

    private IEnumerator shootingEffects()
    {
        yield return new WaitForSeconds(.1333f);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        RaycastHit[] hits = Physics.RaycastAll(playerHead.transform.position,
          playerHead.transform.forward, 600);
        if (hits.Length > 0)
        {
            GameObject sightedObject = playerHead.inSights(hits);
            if (sightedObject.TryGetComponent(out EnemyImproved target))
            {
                target.TakeDamage(5);
            }
            if (sightedObject.TryGetComponent(out EnemyHead enemyHead))
            {
                enemyHead.enemy.TakeDamage(35);
            }
        }

        curAmmo -= 1;
        UpdateAmmoText();
        FindObjectOfType<ReticleLogic>().InitiateReticle(fireTime);
        playerHead.iniateRecoil();
        shootingSFXSource.clip = shootingSFX;
        shootingSFXSource.Play();

    }

    public void Holster() 
    {
        gunAnimator.SetInteger("animState", 3);
        curState = Gunplay.Holster;
    }

    public bool CanHolster()
    {
        return curState == Gunplay.Readied && !isZooming;
    }


    public void UnHolster()
    {
        gunAnimator.SetInteger("animState", 0);
        curState = Gunplay.Readied;
        coolDownTime = 0;
    }


    public bool CanThrowDynamite()
    {
        return !isZooming;
    
    }
}
