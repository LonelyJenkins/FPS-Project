using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HumanController : MonoBehaviour
{
    [Header("General Settings")]
    public LayerMask attackableLayers;
    public bool hasUzi;
    public int health = 100;
    public float detectionRange = 20.0f;
    public float chaseRange = 10.0f;
    public float retreatRange = 1.0f;
    public float visionRange = 15.0f;
    public float accuracyOffset = 0.1f;
    public float despawnTime = 4.0f;
    public LayerMask ignoreLayers;
    public GameObject visionPoint;
    public GameObject rig;
    public GameObject dropItem;
    public bool isAlerted;
    public bool isTDM;
    [Space()]

    [Header("Weapon Settings")]
    public int maxAmmo = 30;
    public int currentAmmo;
    public int damageInflict = 15;
    public float fireRate = 40.0f;
    public float reloadTime = 3.0f;
    public float impactForce = 1000.0f;
    public ParticleSystem muzzleFlash;
    public GameObject muzzleLight;
    public GameObject bodyHitFX;
    public GameObject impactFX;
    [Space()]

    private NavMeshAgent agent;
    private Animator humanAnim;
    private float nextFire = 0;
    private bool isWalking = false;
    [SerializeField] private bool enemyInSight = false;
    private bool isReloading = false;
    public bool isDead = false;
    private string weaponType;
    private PlayerController player;
    private TDMManager tdmManager;


    // Start is called before the first frame update
    void Start()
    {

    }

    private void Awake()
    {
        agent = gameObject.GetComponent<NavMeshAgent>();
        humanAnim = gameObject.GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        DoRagdoll(isDead);
        currentAmmo = maxAmmo;

        AnimationTypeSet(); //this specifies whether uzi specific animations will play, or rifle specific

        if (isTDM) //assigning unique settings for TDM game mode 
        {
            tdmManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TDMManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        humanAnim.SetBool(("alert" + weaponType), isAlerted);
        humanAnim.SetBool(("reload" + weaponType), isReloading);
        humanAnim.SetBool("isWalking", isWalking);

        if (Physics.CheckSphere(transform.position, detectionRange, attackableLayers))
        {
            isAlerted = true;
        }

        if (currentAmmo <= 0 && !isReloading)
        {
            isReloading = true;
            StartCoroutine(Reload());
        }

        if (isAlerted && !isDead)
        {
            FindClosestTarget();

            RaycastHit hit;

            if (Physics.Raycast(visionPoint.transform.position, transform.forward, out hit, visionRange, ~ignoreLayers))
            {
                PlayerController _player = hit.transform.GetComponent<PlayerController>();
                ZombieController zombie = hit.transform.GetComponent<ZombieController>();
                SurvivorController survivor = hit.transform.GetComponent<SurvivorController>();
                BossController boss = hit.transform.GetComponent<BossController>();
                if (survivor != null || zombie != null || _player != null || boss != null)
                {
                    enemyInSight = true;
                }

                else
                {
                    enemyInSight = false;
                }

                if (Time.time >= nextFire && currentAmmo > 0 && !isReloading && enemyInSight)
                {
                    nextFire = Time.time + 1 / fireRate;
                    Shoot();
                }
            }
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
                Vector3 targetPosition = new Vector3(closestTarget.transform.position.x, gameObject.transform.position.y, closestTarget.transform.position.z);
                transform.LookAt(targetPosition);

            if (distanceToNearestTarget > chaseRange)
            {//Chase enemy
                agent.SetDestination(closestTarget.transform.position);
                isWalking = true;
                humanAnim.SetFloat("walkSpeed", 1);
            }

            else if (enemyInSight && distanceToNearestTarget > retreatRange)
            {//Position to stop and shoot as long as enemy is visible
                int dir = Random.Range(1, 3);
                isWalking = true;
                Strafe(dir, closestTarget);
            }

            else if (distanceToNearestTarget < retreatRange)
            {//move away from enemy if too close
                Vector3 dirToTarget = (transform.position - closestTarget.transform.position);
                Vector3 newPos = transform.position + dirToTarget;
                agent.SetDestination(newPos);
                isWalking = true;
                humanAnim.SetFloat("walkSpeed", -1);
            }
        }

        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            agent.SetDestination(player.transform.position);
        }
    }

    void Shoot()
    {
        muzzleFlash.Play();
        StartCoroutine(muzzleLighting());
        currentAmmo--;
        StartCoroutine(ShootAnim());

        Vector3 shootDirection = visionPoint.transform.forward;
            shootDirection.x += Random.Range(-accuracyOffset, accuracyOffset);
            shootDirection.y += Random.Range(-accuracyOffset, accuracyOffset);
            shootDirection.z += Random.Range(0, accuracyOffset);

        RaycastHit hit;

        if (Physics.Raycast(visionPoint.transform.position, shootDirection, out hit, 100, ~ignoreLayers))
        {

            ZombieController zombie = hit.transform.GetComponent<ZombieController>();
            if (zombie != null)
            {
                zombie.TakeDamage(damageInflict);
                GameObject bodyHit = Instantiate(bodyHitFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bodyHit, 1);
            }

            PlayerController _player = hit.transform.GetComponent<PlayerController>();
            if (_player != null)
            {
                _player.TakeDamage(damageInflict);
                GameObject bodyHit = Instantiate(bodyHitFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bodyHit, 1);
            }

            SurvivorController survivor = hit.transform.GetComponent<SurvivorController>();
            if (survivor != null)
            {
                survivor.TakeDamage(damageInflict);
                GameObject bodyHit = Instantiate(bodyHitFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bodyHit, 1);
            }

            BossController boss = hit.transform.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damageInflict);
                GameObject bodyHit = Instantiate(bodyHitFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bodyHit, 1);
            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            GameObject impactObject = Instantiate(impactFX, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactObject, 1);

        }
    }

    IEnumerator Reload()
    {
            Debug.Log("RELOADING HERE");
            yield return new WaitForSeconds(reloadTime);
            Debug.Log("OKAY IM RELOADED");
            isReloading = false;
            currentAmmo = maxAmmo;
    }

    IEnumerator ShootAnim()
    {
        humanAnim.SetBool("shoot" + weaponType, true);
        yield return new WaitForSeconds(0.15f);
        humanAnim.SetBool("shoot" + weaponType, false);
    }

    public void TakeDamage(int amount)
    {
        isAlerted = true;

        health -= amount;
        if (health <= 0)
        {
            Death();
        }
    }

    void Death()
    {
        isDead = true;
        DropItem();
        Collider collider = gameObject.GetComponent<Collider>();
        collider.enabled = false;
        humanAnim.enabled = false;
        agent.enabled = false;
        Destroy(gameObject, despawnTime);
        DoRagdoll(isDead);

        if (isTDM)
        {
            tdmManager.friendlyScore += tdmManager.killValue;
            tdmManager.SpawnNext(1);
        }
    }

    void DoRagdoll(bool deathState)
    {
        Collider[] colliders = rig.GetComponentsInChildren<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = deathState;
        }

        Rigidbody[] rigidbodies = rig.GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = !deathState;
        }
    }

    void AnimationTypeSet()
    {
        if (hasUzi)
        {
            weaponType = "Uzi";
            int animNumber = Random.Range(1, 3);
            RandomIdleAnim(animNumber);
        }
        else
        {
            weaponType = "Rifle";
            int animNumber = Random.Range(1, 4);
            RandomIdleAnim(animNumber);
        }
    }

    void Strafe(int strafeDir, Collider closestTarget)
    {
        if (strafeDir == 1 && closestTarget != null)
        {
            var offsetEnemy = transform.position - closestTarget.transform.position;
            var dir = Vector3.Cross(offsetEnemy, Vector3.up);
            agent.SetDestination(transform.position + dir);
        }

        else if (strafeDir != 1 && closestTarget != null)
        {
            var offsetEnemy = closestTarget.transform.position - transform.position;
            var dir = Vector3.Cross(offsetEnemy, Vector3.up);
            agent.SetDestination(transform.position + dir);
        }
    }

    void RandomIdleAnim(int animNumber)
    {
        {//random selection of idle animations with varying animation speeds
            humanAnim.speed = Random.Range(0.8f, 1.4f);
            humanAnim.SetBool("idle" + animNumber, true);
        }
    }

    void DropItem()
    {
        int dropChance = Random.Range(1, 3);
        if(dropChance == 1)
        {
            Instantiate(dropItem, new Vector3 (transform.position.x, transform.position.y + 1.5f, transform.position.z), Quaternion.identity);
            Debug.Log("AYY YOU GOT SOMETHING");
        }
        else
        {
            Debug.Log("Better Luck Next Kill");
        }
    }

    IEnumerator muzzleLighting()
    {
        Light light = muzzleLight.GetComponent<Light>();
        light.range = (Random.Range(3, 9));
        muzzleLight.SetActive(true);
        yield return new WaitForSeconds(muzzleFlash.main.duration / 2.5f);
        muzzleLight.SetActive(false);
    }
}
