using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public bool isAlerted;
    public Transform alarmCheck;
    public LayerMask attackableLayers;
    public LayerMask damageableLayers;
    public int health = 100;
    public bool isDead = false;
    public GameObject rig;
    public GameObject dropItem;
    public Transform attackPosition;
    public float attackDistance;
    public float attackRange = 1;
    public int damageDealt = 20;
    public float despawnTime = 4.0f;
    public bool isSurvival;
    public bool isChaos;

    private NavMeshAgent agent;
    private Animator zombieAnim;
    private PlayerController playerController;
    private GameManager gameManager;
    private CHAOSManager chaosManager;
    private bool hasBeenShot = false;
    private bool hasAttacked = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        if (isSurvival)
        {
            gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
            isAlerted = true;
        }
        else if (isChaos)
        {
            chaosManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<CHAOSManager>();
        }

        agent = gameObject.GetComponent<NavMeshAgent>();
        zombieAnim = gameObject.GetComponentInChildren<Animator>();
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        //ragdoll state is set inactive
        DoRagdoll(isDead);

        RandomIdleAnim();
    }

    // Update is called once per frame
    void Update()
    {
        zombieAnim.SetBool("isAlerted", isAlerted);

           if (Physics.CheckSphere(alarmCheck.position, 35, attackableLayers) || hasBeenShot)
           {
               isAlerted = true;
           }

           if (isAlerted && !isDead)
           {
               FindClosestTarget();
           }

        if (Physics.CheckSphere(gameObject.transform.position, attackRange, damageableLayers) && isAlerted && !hasAttacked)
        {
            Attack();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            gameObject.transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);
        }
    }

    void RandomIdleAnim()
    {//random selection of idle animations with varying animation speeds
        int idleAnimation = Random.Range(1, 4);
        zombieAnim.speed = Random.Range(0.8f, 1.4f);
        zombieAnim.SetBool("isIdle" + idleAnimation, true);
    }

    public void TakeDamage(int amount, bool playerAttack)
    {
        hasBeenShot = true;
        health -= amount;
        if (health <= 0)
        {
            Death(playerAttack);
            return;
        }

        playerAttack = false;
    }

    void FindClosestTarget()
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

        if (closestTarget != null)
        {
            agent.SetDestination(closestTarget.transform.position);
        }

        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            agent.SetDestination(player.transform.position);
        }
    }


    void Attack()
    {
        StartCoroutine(AttackReload(1));
        int attackType = Random.Range(1, 3);
        zombieAnim.SetTrigger("Attack" + attackType);
        hasAttacked = true;
        Collider[] target = Physics.OverlapSphere(attackPosition.position, attackDistance, damageableLayers);
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
                door.TakeDamage(damageDealt);
            }
        }
        return;
    }

    void Death(bool playerAttack)
    {
        isDead = true;

        if (isSurvival)
        {
            gameManager.enemyCount--;
            if (playerAttack == true)
            {
                gameManager.playerKillCount++;
            }
        }

        else if (isChaos && playerAttack == true)
        {
            chaosManager.playerKillCount++;
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
    {
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

    IEnumerator AttackReload(int reloadTime)
    {
        yield return new WaitForSeconds(reloadTime);
        hasAttacked = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(attackPosition.position, attackDistance);
    }
}
