using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyImproved : MonoBehaviour
{

    public enum FSMStates 
    {
        idle,
        patrol,
        chasing,
        shooting,
        dead,
    }


    GameObject player;
    public int maxHealth = 30;
    int curhealth = 30;
    public bool isDead = false;
    public bool canShoot = false; // New variable to control shooting
    public float shootingCooldown = 1;
    public int enemyDamageAmount = 5;
    float shootingTime = 0;
    public float seeingRadius = 5;
    public float hearingRadius = 10;
    public float fieldOfView = 45f;
    public Transform head;
    public string displayName;
    public FSMStates currentState;
    public GameObject[] wanderPoints;
    public GameObject gun;
    Vector3 lastKnownPlayerLocation;
    NavMeshAgent agent;

    public Animator playerAnimator;
    Vector3 nextDestination;
    int currentDestinationIndex = 0;
    float distanceToPlayer;

    //For Level 1 - set both to false in level1 inspecter scene
    public bool gamePlayed = true;
    public bool gameWon = true;
    public static bool cardGamePlayed; // Flag to track card game completion
    public static bool playerWonCardGame; // Flag to track card game winner


    // Start is called before the first frame update
    void Start()
    {
        curhealth = maxHealth;
        playerWonCardGame = gameWon;
        cardGamePlayed = gamePlayed;
        //currentState = FSMStates.idle;
        player = GameObject.FindGameObjectWithTag("Player");
        agent = GetComponent<NavMeshAgent>();
        if (wanderPoints.Length > 1)
        {
            currentState = FSMStates.patrol;
            FindNextPoint();
        }
        else 
        {
            GetComponent<NavMeshAgent>().enabled = false;
        }
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!cardGamePlayed) // Don't process enemy states until card game is played
            return;

        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        switch(currentState)
        {
            case FSMStates.idle:
                UpdateIdleState();
                break;
            case FSMStates.patrol:
                UpdatePatrolState();
                break;
            case FSMStates.shooting:
                UpdateShootState();
                break;
            case FSMStates.chasing:
                UpdateChasingState();
                break;
            case FSMStates.dead:
                UpdateDeadState();
                break;
        }
        
        if (curhealth <= 0 && currentState != FSMStates.dead)
        {
            currentState = FSMStates.dead;
            GameObject gm = GameObject.FindGameObjectWithTag("EnemyCount");
            if (gm != null && gm.TryGetComponent(out EnemyCounter counter))
            {
                counter.RemoveEnemy();
            }
        }

    }

    void UpdateIdleState()
    {
        playerAnimator.SetInteger("animState", 0);
        if (distanceToPlayer <= seeingRadius
           && IsPlayerInClearFOV())
        {
            currentState = FSMStates.shooting;
        }
    }

    void UpdateChasingState()
    {
        playerAnimator.SetInteger("animState", 1);
        nextDestination = lastKnownPlayerLocation;
        if (Vector3.Distance(transform.position, nextDestination) < 3)
        {
            if (this.wanderPoints.Length > 1)
            {
                FindNextPoint();
                currentState = FSMStates.patrol;
            }
            else 
            {
                currentState = FSMStates.idle;
            }
        }
        else if (distanceToPlayer <= seeingRadius
        && IsPlayerInClearFOV())
        {
            currentState = FSMStates.shooting;
        }

        FaceTarget(nextDestination);
    }


    void UpdatePatrolState()
    {
        playerAnimator.SetInteger("animState", 1);

        if(Vector3.Distance(transform.position, nextDestination) < 3)
        {
            FindNextPoint();
        }
        else if (distanceToPlayer <= seeingRadius 
        && IsPlayerInClearFOV()) 
        {
            currentState = FSMStates.shooting;
        }

        FaceTarget(agent.steeringTarget);

    }

    // Updates Shooting FSM State
    void UpdateShootState()
    {
        agent.isStopped = true;
        gun.SetActive(true);
        playerAnimator.SetInteger("animState", 5);
        var playerPos = player.transform.position;
        playerPos.y = transform.position.y;
        transform.LookAt(playerPos);
        transform.Rotate(0, 30, 0);

        if (head != null)
        {
            head.transform.LookAt(player.transform.position);
        }

        if (Vector3.Distance(player.transform.position, transform.position) > (seeingRadius + hearingRadius) / 2
            || !IsPlayerInClearFOV()
        )
        {
            agent.isStopped = false;
            lastKnownPlayerLocation = player.transform.position;
            currentState = FSMStates.chasing;
            agent.SetDestination(lastKnownPlayerLocation);
            return;
        }
        RaycastHit[] hits;
        if (head != null)
        {
            hits = Physics.RaycastAll(head.transform.position, head.transform.forward, 50);
        }
        else
        {
            hits = Physics.RaycastAll(transform.position, transform.forward, 50);
        }

        if (canShoot && InSights(hits) == player)
        {
            if (shootingTime <= 0)
            {
                ShootPlayer();
            }
            if (shootingTime > 0)
            {
                shootingTime -= Time.deltaTime;
            }
        }
        else
        {
            if (shootingTime > shootingCooldown / 2)
            {
                shootingTime -= Time.deltaTime;
            }
            else if (shootingTime < shootingCooldown / 2)
            {
                shootingTime += Time.deltaTime / 2;
            }
        }
    }

    // Updates Dead FSM State
    void UpdateDeadState()
    {
        agent.isStopped = true;
        isDead = true;
        if (playerAnimator.GetInteger("animState") != 2)
        {
            playerAnimator.SetInteger("animState", 2);
            this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY;
            if (FindAnyObjectByType<EnemyManager>() != null)
            {
                FindAnyObjectByType<EnemyManager>().enemyDied();
            }
        }
    }

    // Sets the enemy's destination to be the next wanderpoint in the array
    void FindNextPoint()
    {
        nextDestination = wanderPoints[currentDestinationIndex].transform.position;

        currentDestinationIndex = (currentDestinationIndex + 1) % wanderPoints.Length;

        agent.SetDestination(nextDestination);
    }

    // Rotates the Enemy to face the given target
    void FaceTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;

        directionToTarget.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10 * Time.deltaTime);
    }

    // Rotates the Enemy to face the given target
    void FaceTargetRapid(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;

        directionToTarget.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

        transform.rotation = lookRotation;
    }


    /// <summary>
    /// Receives the given amount of damage. If the enemy's health goes below 0, it will fling itself backwards and be considered "dead"
    /// </summary>
    /// <param name="damage"></param>
    public void TakeDamage(int damage)
    {
        if (isDead) { return; }

        curhealth -= damage;
        if (!isDead && curhealth <= 0)
        {
            curhealth = 0;
            //this.transform.Rotate(-30f, 0f, 0f);
            //Rigidbody rb = this.GetComponent<Rigidbody>();
            //rb.constraints -= RigidbodyConstraints.FreezeRotationX;
            //rb.constraints -= RigidbodyConstraints.FreezeRotationZ;
            //rb.AddForce((transform.forward + Vector3.down) * -500);
            UpdateDeadState();
        }
    }

    /// <summary>
    /// Returns the closest gameobject in the given list of hits, returning null if no collisions are made
    /// </summary>
    private GameObject InSights(RaycastHit[] hits)
    {
        GameObject shortestGameObject = null;
        float shortestDistance = float.MaxValue;
        foreach (RaycastHit hit in hits)
        {
            if (hit.distance < shortestDistance && hit.collider.gameObject.tag != this.tag)
            {
                shortestDistance = hit.distance;
                shortestGameObject = hit.collider.gameObject;
            }
        }
        return shortestGameObject;
    }

    private void ShootPlayer()
    {
        this.GetComponent<AudioSource>().Play();

        if (Random.Range(0, 5) < 2)
        {
            player.GetComponent<PlayerController>().TakeDamage(enemyDamageAmount);
        }

        shootingTime = shootingCooldown;
    }

    public void OnPlayerFire() 
    {
        if (currentState == FSMStates.shooting || isDead) return;
        Debug.Log(Vector3.Distance(player.transform.position, transform.position));
        if (Vector3.Distance(player.transform.position, transform.position) <= hearingRadius) 
        {
            GameObject sightedGameObject = InSights(Physics.RaycastAll(head.position, player.transform.position - head.position, hearingRadius));
            if (sightedGameObject != null) 
            {
                Debug.Log(transform.parent.gameObject.name + "|" + sightedGameObject.name);
            }
            if (sightedGameObject != null && sightedGameObject.TryGetComponent(out PlayerController PC))
            {
                FaceTargetRapid(player.transform.position);
                currentState = FSMStates.shooting;
            }
        }
    }

    public void HealthDisplay() 
    {
        if (isDead) return;
        Slider slider = GameObject.FindGameObjectWithTag("EnemyHealthSlider").GetComponent<Slider>();
        slider.gameObject.GetComponent<CanvasGroup>().alpha = 1;
        slider.value = curhealth / (float) maxHealth;
        TMP_Text text = GameObject.FindGameObjectWithTag("EnemyDisplayName").GetComponent<TMP_Text>();
        text.gameObject.GetComponent<CanvasGroup>().alpha = 1;
        text.text = displayName;
    }

    bool IsPlayerInClearFOV()
    {
        RaycastHit hit;
        Vector3 directionToPlayer = player.transform.position - head.position;
        
        if (Vector3.Angle(directionToPlayer, head.forward) <= fieldOfView)
        {
            if(Physics.Raycast(head.position, directionToPlayer, out hit, seeingRadius))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public static void SetCardGamePlayed(bool win) // Function to signal the card game is played and result (win/lose)
    {
        cardGamePlayed = true;
        playerWonCardGame = win;
    }
}
