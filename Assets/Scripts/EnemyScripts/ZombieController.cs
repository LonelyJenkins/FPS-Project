using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public bool isAlerted;
    public Transform alarmCheck; //distance from which zombie will be triggered if in idle state
    public LayerMask attackableLayers;
    public LayerMask damageableLayers;
    public int health = 100;
    public bool isDead = false;
    public AudioClip biteSFX;
    public AudioClip attackSFX;
    public AudioClip[] groanSFX;
    public AudioClip deathSFX;
    public GameObject rig; //the part of the gameObject that is parent to all of the parts of the gameObject. This is to activate ragdoll state on death
    public GameObject dropItem;
    public Transform attackPosition; //points on prefab that inflict damage on other attackable types (ex. hands)
    public float attackDistance;//the radius of which the attack positions can affect
    public float attackRange = 1;//the distance from the zombie an attackable must be before an attack is triggered
    public int damageDealt = 20; //value of damage against attackable
    public float despawnTime = 4.0f; //length of time before prefab is destroyed after death state is triggered
    public bool isSurvival = false; //game mode check
    public bool isChaos = false;// see above

    private NavMeshAgent agent;
    private Animator zombieAnim;
    private AudioSource zombieAudio;
    private PlayerController playerController;
    private Transform playerPosition;
    private GameManager gameManager;
    private CHAOSManager chaosManager;
    private Identifier identifier;
    private float groanInterval;
    private bool hasBeenShot = false;
    private bool hasAttacked = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        GameObject manager = GameObject.FindGameObjectWithTag("GameManager");
        zombieAudio = gameObject.GetComponent<AudioSource>();
        identifier = manager.GetComponentInChildren<Identifier>(); //checking for game mode type


        if (identifier.isChaos)
        {
            isChaos = true;
            isSurvival = false;
            chaosManager = manager.GetComponent<CHAOSManager>();
        }

        if (identifier.isSurvival)
        {
            isSurvival = true;
            isChaos = false;
            isAlerted = true;
            gameManager = manager.GetComponent<GameManager>();
        }

        agent = gameObject.GetComponent<NavMeshAgent>();
        zombieAnim = gameObject.GetComponentInChildren<Animator>();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        //ragdoll state is set inactive
        DoRagdoll(isDead);

        RandomIdleAnim();

        groanInterval = Time.time + Random.Range(20, 40); //this determines the intervals in which the zombie will emit a groan
    }

    // Update is called once per frame
    void Update()
    {
        zombieAnim.SetBool("isAlerted", isAlerted);
        playerPosition = playerController.transform; //actively keeping track of player position in case other attackables are not in attack range

           if (Physics.CheckSphere(alarmCheck.position, 35, attackableLayers) || hasBeenShot) //if the zombie has been attacked or alert radius has been entered then zombie will enter aggro state
           {
               isAlerted = true;
           }

           if (isAlerted && !isDead) //actively searches for nearest target once aggro state is active
           {
               FindClosestTarget();
           }

        if (Physics.CheckSphere(gameObject.transform.position, attackRange, damageableLayers) && isAlerted && !hasAttacked) //if attackable is in the attack radius, an attack is triggered as long as the attack reload has been complete
        {
            Attack();
        }

        if (Time.time >= groanInterval) //timer for groan sfx
        {
            TriggerGroan();
            groanInterval = Time.time + Random.Range(20, 40);
        }

        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            gameObject.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f); //prevents zombies from clipping through doors in survival mode
        }
    }

    void RandomIdleAnim()
    {//machine that determines a random idle animation on start
        int idleAnimation = Random.Range(1, 4);
        zombieAnim.speed = Random.Range(0.8f, 1.4f);
        zombieAnim.SetBool("isIdle" + idleAnimation, true);
    }

    void TriggerGroan()
    {
        int randomGroan = Random.Range(0, groanSFX.Length); //choosing random groan sfx to play
        zombieAudio.pitch = Random.Range(0.8f, 1.1f); //adding variance to groan sfx
        zombieAudio.PlayOneShot(groanSFX[randomGroan]);
    }

    public void TakeDamage(int amount, bool playerAttack) //takes value of incoming damage and checks if damage was from player.
    {
        hasBeenShot = true;
        health -= amount;
        if (health <= 0)
        {
            Death(playerAttack); //if killing blow was given by player, this bool is given to the death state
            return;
        }
    }

    void FindClosestTarget() //checking all targets within alarm radius to determine the closest one. That one is then chosen as the destination
    {
        float distanceToNearestTarget = Mathf.Infinity;
        Collider closestTarget = null;

        Collider[] nearbyTargets = Physics.OverlapSphere(alarmCheck.position, 30, attackableLayers);
        foreach (Collider attackables in nearbyTargets)
        {
            float distanceToTarget = Vector3.Distance(attackables.transform.position, gameObject.transform.position);
            if (distanceToTarget < distanceToNearestTarget)
            {
                distanceToNearestTarget = distanceToTarget;
                closestTarget = attackables;
            }
        }

        if (closestTarget != null) //if a target is in range then zombie will approach it
        {
            agent.SetDestination(closestTarget.transform.position);
        }

        else //if no targets are in range then player is default target
        {
            agent.SetDestination(playerPosition.position);
        }
    }


    void Attack()
    {
        StartCoroutine(AttackReload(1)); //timer is triggered to reset attack after initial attack has been triggered
        int attackType = Random.Range(1, 3); //machine determines a random attack animation
        zombieAnim.SetTrigger("Attack" + attackType);
        hasAttacked = true;

        if (attackType == 1) //determining the appropriate audio SFX for the specific attack type
        {
            zombieAudio.PlayOneShot(attackSFX);
        }
        else
        {
            zombieAudio.PlayOneShot(biteSFX);
        }

        Collider[] target = Physics.OverlapSphere(attackPosition.position, attackDistance, damageableLayers); //checking for targets hit, if any
        foreach (Collider attackable in target)
        {
            Debug.Log(gameObject.name + " Has hit " + attackable.name);

            HumanController human = attackable.GetComponent<HumanController>();
            if (human != null)
            {
                human.TakeDamage(damageDealt, false);
            }

            PlayerController _player = attackable.GetComponent<PlayerController>();
            if (_player != null)
            {
                _player.TakeDamage(damageDealt);
            }

            SurvivorController survivor = attackable.GetComponent<SurvivorController>();
            if (survivor != null)
            {
                survivor.TakeDamage(damageDealt);
            }

            DoorController door = attackable.GetComponent<DoorController>();
            if (door != null)
            {
                door.TakeDamage(damageDealt); //zombies can damage doors in survival mode. Ultimately can destroy cabin
            }
        }
        return;
    }

    void Death(bool playerAttack) //death state. Checks if killing blow was made by player
    {
        zombieAudio.PlayOneShot(deathSFX);

        isDead = true;

        if (isSurvival)
        {
            gameManager.enemyCount--;
            if (playerAttack == true)
            {
                gameManager.playerKillCount++;
            }
        }

        else if (isChaos)
        {
            chaosManager.zombieKillCounter++;
            if (playerAttack == true)
            {
                chaosManager.playerKillCount++;
            }
        }

        DropItem();
        Collider collider = gameObject.GetComponent<Collider>();
        collider.enabled = false;
        Destroy(gameObject, despawnTime);
        agent.enabled = false;
        zombieAnim.enabled = false;
        DoRagdoll(isDead);
    }

    void DoRagdoll(bool deathState)
    {//ragdoll state will be triggered with isDead boolean
        Collider[] colliders = rig.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = deathState;
        }

        Rigidbody[] rbs = gameObject.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rigidBodies in rbs)
        {
            rigidBodies.isKinematic = !deathState;
        }
    }

    void DropItem()
    { //machine that determines if an item is dropped after death state is triggered
        int dropChance = Random.Range(1, 4);
        if (dropChance == 1)
        {
            Instantiate(dropItem, new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z), Quaternion.identity);
        }
        else
        {
            Debug.Log("Better Luck Next Kill BITCH");
        }
    }

    IEnumerator AttackReload(int reloadTime) //timer that determines how long until another attack can be performed
    {
        yield return new WaitForSeconds(reloadTime);
        hasAttacked = false;
    }

    private void OnDrawGizmosSelected() //visible representation of how large the attack radius is. This is specifically for Unity editor
    {
        Gizmos.DrawWireSphere(attackPosition.position, attackDistance);
    }
}
