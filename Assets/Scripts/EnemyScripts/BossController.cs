using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    public bool isAlerted = true;
    public bool isDead = false;
    public bool isSurvival = false;
    public bool isChaos = false;
    public GameObject rig;
    public GameObject dropItem;
    public int health = 500;
    public int damageDealt = 25;
    public float despawnTime = 5.0f;
    public float detectionRange = 50.0f;
    public GameObject[] attackPositions;
    public float attackDistance = 1.0f;
    public float attackRange = 4.0f;
    public float hitForce = 1000.0f;
    public AudioClip roarSFX;
    public AudioClip deathSFX;
    public AudioClip attackSFX;

    public LayerMask attackableLayers;
    public LayerMask damageableLayers;

    private NavMeshAgent agent;
    private Animator bossAnim;
    private AudioSource bossAudio;
    private GameManager gameManager;
    private CHAOSManager chaosManager;
    private Identifier identifier;
    private bool hasAttacked = false;


    private void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        bossAnim = gameObject.GetComponent<Animator>();
        bossAudio = gameObject.GetComponent<AudioSource>();
        DoRagdoll(isDead, true);
        isAlerted = true;

        GameObject manager = GameObject.FindGameObjectWithTag("GameManager");
        identifier = manager.GetComponentInChildren<Identifier>();

        if (identifier.isChaos == true)
        {
            isChaos = true;
            isSurvival = false;
            chaosManager = manager.GetComponent<CHAOSManager>();
        }

        if (identifier.isSurvival == true)
        {
            isSurvival = true;
            isChaos = false;
            gameManager = manager.GetComponent<GameManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        bossAnim.SetBool("isAlerted", isAlerted);

        if (Physics.CheckSphere(transform.position, detectionRange, attackableLayers))
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
            Attack();
        }
    }

    void Attack()
    {
        StartCoroutine(AttackReload(1));
        int attackType = Random.Range(1, 3);
        bossAnim.SetTrigger("attack" + attackType);
        hasAttacked = true;
    }

    public void TakeDamage(int amount, bool playerAttack)
    {
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

        Collider[] nearbyTargets = Physics.OverlapSphere(gameObject.transform.position, 30, attackableLayers);
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
            Vector3 targetPosition = new Vector3(closestTarget.transform.position.x, gameObject.transform.position.y, closestTarget.transform.position.z);
            transform.LookAt(targetPosition);
        }

        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            agent.SetDestination(player.transform.position);
        }
    }

    void Death(bool playerAttack)
    {
        isDead = true;
        bossAudio.PlayOneShot(deathSFX);
        DropItem();
        Collider collider = gameObject.GetComponent<Collider>();
        collider.enabled = false;
        Destroy(gameObject, despawnTime);
        agent.enabled = false;
        bossAnim.enabled = false;
        DoRagdoll(isDead, true);

        if(isSurvival)
        {
            gameManager.bossCount--;
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
    }

    void DoRagdoll(bool deathState, bool canAttack)
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

        foreach (GameObject obj in attackPositions)
        {
            BossAttack attack = obj.GetComponent<BossAttack>();
            Collider col = obj.GetComponent<Collider>();
            Rigidbody rb = obj.GetComponent<Rigidbody>();

            attack.enabled = !deathState;
            col.enabled = !deathState;
            rb.isKinematic = canAttack;
        }
    }

   public void AttackForce(Vector3 collisionPoint)
    {
        Collider[] collidersToMove = Physics.OverlapSphere(transform.position, attackDistance);

        foreach (Collider nearbyObject in collidersToMove)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(hitForce, collisionPoint, attackDistance, 1, ForceMode.Impulse);
            }
        }
    }

    public void AttackSFXTrigger()
    {
        bossAudio.PlayOneShot(attackSFX); //method created to be called from bossAttack script
    }

    void DropItem()
    {
        int dropChance = Random.Range(1, 3);
        if (dropChance == 1)
        {
            Instantiate(dropItem, new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z), Quaternion.identity);
        }

        else
        {
            return;
        }
    }

    IEnumerator AttackReload(int reloadTime)
    {
        yield return new WaitForSeconds(reloadTime);
        hasAttacked = false;
    }

    private void OnDrawGizmosSelected() //visual representation of the attack range (for unity editor)
    {
        Gizmos.DrawWireSphere(gameObject.transform.position, attackRange);
    }
}
