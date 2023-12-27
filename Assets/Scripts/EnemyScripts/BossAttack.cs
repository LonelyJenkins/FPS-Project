using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    private BossController boss;
    private int damageDealt;
    // Start is called before the first frame update
    void Start()
    {
        boss = gameObject.GetComponentInParent<BossController>();
        damageDealt = boss.damageDealt;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().TakeDamage(damageDealt);
            boss.AttackForce(gameObject.transform.position);
        }

        if (other.CompareTag("Human"))
        {
            other.GetComponent<HumanController>().TakeDamage(damageDealt);
            boss.AttackForce(gameObject.transform.position);
        }

        if (other.CompareTag("Survivor"))
        {
            other.GetComponent<SurvivorController>().TakeDamage(damageDealt);
            boss.AttackForce(gameObject.transform.position);
        }

        if (other.CompareTag("Door"))
        {
            DoorController door = other.GetComponentInChildren<DoorController>();
            if (door != null)
            {
                door.TakeDamage(damageDealt);
            }
        }
    }

}
