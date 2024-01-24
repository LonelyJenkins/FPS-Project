using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketMotion : MonoBehaviour
{
    public float speed = 5;
    public float damageRadius = 5;
    public int damageInflict = 75;
    public float explosionForce = 1000;
    public int lifeTime = 3;

    public GameObject explosionFx;

    private float gravity = -9.81f;
    private Vector3 velocity;

    private void Start()
    {
        StartCoroutine(RocketLifetime());
    }
    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime); //gravitational influence and flight path of the rocket
        velocity.y += gravity * Time.deltaTime;
        transform.Translate(velocity * Time.deltaTime);

    }

    private void OnTriggerEnter(Collider other)
    {
        DetachSmoke();
        Impact();
    }

    void Impact()
    {
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
                rb.AddExplosionForce(explosionForce, transform.position, damageRadius, 4, ForceMode.Impulse);
            }
        }
        Destroy(gameObject);
    }

    IEnumerator RocketLifetime()
    {
        yield return new WaitForSeconds(lifeTime); //explodes rocket if it has not impacted anything for designated timespan

        Impact();
    }

    void DetachSmoke() //leaves smoke particles in scene after parent object has been destroyed
    {
        Transform smokeTrail = transform.Find("SmokeTrail");
        if (smokeTrail != null)
        {
            ParticleSystem smoke = smokeTrail.GetComponentInChildren<ParticleSystem>();
            var emission = smoke.emission;
            emission.rateOverTime = 0;
            smoke.transform.parent = null;
            smoke.transform.localScale = new Vector3(1, 1, 1);
            Destroy(smoke, 5);
        }
    }
}
