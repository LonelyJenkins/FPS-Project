using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float lifeTime = 3;
    public float damageRadius = 3;
    public float explosionForce = 40;
    public int damageInflict = 50;

    public GameObject explosionFx;

    private float countDown;
    private bool hasExploded = false;
    private Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        countDown = lifeTime;
    }

    // Update is called once per frame
    void Update()
    {
        countDown -= Time.deltaTime;
        if (countDown <= 0 && !hasExploded)
        {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            rigidBody.velocity = Vector3.zero;
        }
    }

    void Explode()
    {
        hasExploded = true;

        Instantiate(explosionFx, transform.position, transform.rotation);

        Collider[] collidersToDamage = Physics.OverlapSphere(transform.position, damageRadius);

        foreach (Collider nearbyObject in collidersToDamage)
        {

            ZombieController zombie = nearbyObject.GetComponent<ZombieController>();
            if (zombie != null)
            {
                zombie.TakeDamage(damageInflict, true);
            }

            HumanController human = nearbyObject.GetComponent<HumanController>();
            if (human != null)
            {
                human.TakeDamage(damageInflict, true);
            }

            BossController boss = nearbyObject.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damageInflict, true);
            }
        }

        Collider[] collidersToMove = Physics.OverlapSphere(transform.position, damageRadius);

        foreach (Collider nearbyObject in collidersToMove)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, damageRadius, 2, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }
}
