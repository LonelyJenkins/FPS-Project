using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public float speed = 200.0f; //pickup rotation speed

    [Header("Ammo Type")]
    public bool Uzi;
    public bool AK;
    public bool M4;
    public bool Rocket;
    public bool Grenade;
    public bool health;
    public int healthPickupValue = 20;
    public int despawnTime = 30;
    public GameObject ammoPickupFX;
    public GameObject healthPickupFX;

    private void Start()
    {
        if(health == true)
        {
            StartCoroutine(DespawnTimer());
        }
    }
    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0, speed * Time.deltaTime, 0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !health) //detecting the kind of ammo player is picking up
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.AmmoPickupSFXTrigger(); //playing ammo pickup sfx
            GameObject ammoPickupObject = Instantiate(ammoPickupFX, transform.position, transform.rotation); //spawning particle vfx
            
            Destroy(ammoPickupObject, 1);

            if (Uzi == true)
            {
                player.UziPickup();
            }
            if (AK == true)
            {
                player.AkPickup();
            }
            if (M4 == true)
            {
                player.M4Pickup();
            }
            if (Rocket == true)
            {
                player.RocketPickup();
            }
            if (Grenade == true)
            {
                player.GrenadePickup();
            }
            Destroy(gameObject);
        }

        else if (other.CompareTag("Player") && health)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.HealthPickupSFXTrigger();
            GameObject healthPickupObject = Instantiate(healthPickupFX, transform.position, transform.rotation);
            Destroy(healthPickupObject, 1);
            player.currentHealth += healthPickupValue;
            Destroy(gameObject);
        }
    }

    IEnumerator DespawnTimer()
    {
        yield return new WaitForSeconds(despawnTime);
        Destroy(gameObject);
    }
}
