using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

public class Submachine : MonoBehaviour, IGUN
{
    public AudioClip reloadingSFX;
    Animator gunAnimator;
    public AudioClip shootingSFX;
    AudioSource shootingSFXSource;
    public float reloadTime = 3f;
    public float fireTime = .16666666666f;
    public float holsterTime = 1;
    float coolDownTime = 0f;
    public Gunplay curState;
    public int maxAmmo = 30;
    public float SIGHTINGTIME = .5f;
    float sightingTimer = 0;
    Vector3 startLocalPos;
    public Vector3 desiredZoomPos;
    int curAmmo;
    TMP_Text ammoText;
    MouseLook playerHead;
    float recoilAimOffset = 5f;
    float startFOV;
    public float decreaseInFOV = 20f;
    bool isZooming = false;
    public GameObject bulletPrefab;
    public GameObject bulletTransformParent;
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
        this.transform.localRotation = Quaternion.Euler(new Vector3(0, -5 * (1 - curTime) + 180, 0));

        if ((sightingTimer <= 0))
        {
            isZooming = false;
        }
        else 
        {
            isZooming = true;
        }


        GameObject.FindAnyObjectByType<PlayerController>().sendZoomingSignal(isZooming);
        recoilAimOffset = Mathf.Max(recoilAimOffset - Time.deltaTime, 5);
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




        if (Input.GetButton("Fire1") && curAmmo > 0)
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
        yield return new WaitForSeconds(.0334f);
        Vector3 currentForward = playerHead.transform.forward;
        float split = Random.Range(-recoilAimOffset, recoilAimOffset);
        Vector3 randomRot =  Quaternion.AngleAxis(recoilAimOffset - split, Vector3.right)  * Quaternion.AngleAxis(split, Vector3.up) * currentForward;
        RaycastHit[] hits = Physics.RaycastAll(playerHead.transform.position,
          randomRot, 600);

        GameObject newBullet = Instantiate(bulletPrefab, bulletTransformParent.transform);
        newBullet.GetComponent<Rigidbody>().AddForce(bulletTransformParent.transform.up * 150 * Random.Range(.7f, 1.1f) + bulletTransformParent.transform.right * 200 *  Random.Range(.7f, 1.1f));
        Debug.Log(newBullet.transform.rotation.eulerAngles);
        newBullet.GetComponent<Rigidbody>().AddTorque(bulletTransformParent.transform.forward * 3000 + bulletTransformParent.transform.up * 3000);
        newBullet.transform.parent = null;
        if (hits.Length > 0)
        {
            GameObject sightedObject = playerHead.inSights(hits);
            if (sightedObject.TryGetComponent(out EnemyImproved target))
            {
                target.TakeDamage(5);
            }
            if (sightedObject.TryGetComponent(out EnemyHead enemyHead))
            {
                enemyHead.enemy.TakeDamage(10);
            }
        }
        recoilAimOffset = Mathf.Min(recoilAimOffset + 5, 45);
        curAmmo -= 1;
        UpdateAmmoText();
        FindObjectOfType<ReticleLogic>().InitiateReticle(fireTime);
        playerHead.iniateRecoil(10);
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
