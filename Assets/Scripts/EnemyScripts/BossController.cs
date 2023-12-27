using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonoBehaviour
{
    public bool isAlerted = true;
    public bool isDead = false;
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

    public LayerMask attackableLayers;
    public LayerMask damageableLayers;

    private NavMeshAgent agent;
    private Animator bossAnim;
    private GameManager gameManager;
    private bool hasAttacked = false;


    // Start is called before the first frame update
    void Start()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        bossAnim = gameObject.GetComponent<Animator>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        DoRagdoll(isDead, true);
        isAlerted = true;
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

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Death();
        }
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

    void Death()
    {
        isDead = true;
        gameManager.bossCount--;
        DropItem();
        Collider collider = gameObject.GetComponent<Collider>();
        collider.enabled = false;
        Destroy(gameObject, despawnTime);
        agent.enabled = false;
        bossAnim.enabled = false;
        DoRagdoll(isDead, true);
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

    void DropItem()
    {
        int dropChance = Random.Range(1, 3);
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
        Gizmos.DrawWireSphere(gameObject.transform.position, attackRange);
    }
}
