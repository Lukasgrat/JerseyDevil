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
    GameObject playerHead;
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
        playerHead = Camera.main.gameObject;
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
        if (nextDestination != lastKnownPlayerLocation && agent.isOnNavMesh) 
        {

            agent.SetDestination(lastKnownPlayerLocation);
            nextDestination = lastKnownPlayerLocation;
            agent.isStopped = false;
        }
        if (Vector3.Distance(transform.position, nextDestination) < 3 || agent.pathStatus != NavMeshPathStatus.PathComplete)
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

        FaceTarget(agent.steeringTarget);
    }


    void UpdatePatrolState()
    {
        playerAnimator.SetInteger("animState", 1);

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        if (Vector3.Distance(transform.position, nextDestination) < 3)
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
        if (agent.isOnNavMesh) 
        {
            agent.isStopped = true;
        }
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

        if (Vector3.Distance(player.transform.position, transform.position) > seeingRadius
            || !IsPlayerInClearFOV()
        )
        {
            if (agent.isOnNavMesh) 
            {
                agent.isStopped = false;
            }
            lastKnownPlayerLocation = player.transform.position;
            currentState = FSMStates.chasing;
            if (agent.isOnNavMesh) 
            {
                agent.SetDestination(lastKnownPlayerLocation);
            }
            return;
        }

        if (shootingTime <= 0 && currentState == FSMStates.shooting)
        {
            ShootPlayer();
            foreach (EnemyImproved enemy in GameObject.FindObjectsOfType<EnemyImproved>())
            {
                enemy.OnPlayerFire();
            }
        }
        if (shootingTime > 0)
        {
            shootingTime -= Time.deltaTime;
        }

    }

    // Updates Dead FSM State
    void UpdateDeadState()
    {
        if (GetComponent<NavMeshAgent>().enabled) 
        {
            agent.isStopped = true;
            GetComponent<NavMeshAgent>().enabled = false;
        }
        GetComponent<NavMeshObstacle>().enabled = true;

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

        currentDestinationIndex =  Random.Range(0, wanderPoints.Length);
        if (agent.isOnNavMesh) 
        {
            agent.SetDestination(nextDestination);
        }
    }

    // Rotates the Enemy to face the given target
    void FaceTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;

        directionToTarget.y = 0;
        if (directionToTarget != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 10 * Time.deltaTime);
        }
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
            if (hit.distance < shortestDistance && hit.collider.gameObject.tag != this.tag && hit.collider.gameObject.tag != "Gun")
            {
                shortestDistance = hit.distance;
                shortestGameObject = hit.collider.gameObject;
            }
        }
        return shortestGameObject;
    }

    private void ShootPlayer()
    {
        //AudioSource.PlayClipAtPoint(this.GetComponent<AudioSource>().clip, this.transform.position);
        this.GetComponent<AudioSource>().PlayOneShot(this.GetComponent<AudioSource>().clip);
        if (Random.Range(0, 5) < 2)
        {
            player.GetComponent<PlayerController>().TakeDamage(enemyDamageAmount);
        }

        shootingTime = shootingCooldown;
    }

    public void OnPlayerFire() 
    {
        if (currentState == FSMStates.shooting || isDead) return;
        if (Vector3.Distance(player.transform.position, transform.position) <= hearingRadius) 
        {
            GameObject sightedGameObject = InSights(Physics.RaycastAll(head.position, player.transform.position - head.position, seeingRadius));
            if (sightedGameObject != null && sightedGameObject.TryGetComponent(out PlayerController PC))
            {
                FaceTargetRapid(player.transform.position);
                currentState = FSMStates.shooting;
            }
            else 
            {
                currentState = FSMStates.chasing;
                lastKnownPlayerLocation = player.transform.position;
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
        if (Vector3.Distance(head.position, player.transform.position) > seeingRadius) 
        {
            return false;
        }
        if (Vector3.Angle(directionToPlayer, head.forward) <= fieldOfView)
        {
            GameObject sightedGameObject = InSights(Physics.RaycastAll(head.position, player.transform.position - head.position, seeingRadius));
            GameObject sightedBasedOnHead= InSights(Physics.RaycastAll(head.position, playerHead.transform.position - head.position, seeingRadius));
            if (sightedGameObject != null) 
            {
                if(sightedGameObject.CompareTag("Player"))
                return true;
            }
            if (sightedBasedOnHead != null)
            {
                return sightedBasedOnHead.CompareTag("Player");
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
