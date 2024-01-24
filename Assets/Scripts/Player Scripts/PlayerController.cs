using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Controller Settings")]
    public CharacterController controller;
    public Transform groundCheck;
    public LayerMask groundMask;
    public float doorCheckDistance = 2.0f;
    public LayerMask doorLayer;
    public float speed = 12;
    public float gravity = -9.81f;
    public float jumpHeight = 3;
    [Space()]

    [Header("Player Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public bool isDead = false;
    public GameObject gunCam;
    [Space()]

    [Header("GameMode Settings")]
    public bool isSurvival = false;
    public bool isTDM = false;
    public Transform[] spawnPoints;

    private float groundDistance = 0.4f;
    private bool isGrounded;
    private bool isPointingAtDoor;
    private TDMManager tdmManager;
    private Camera mainCam;
    private Vector3 velocity;
    private GameObject AK;
    private GameObject Uzi;
    private GameObject Colt;
    private GameObject SMAW;
    private GameObject Grenade;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        AK = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/AkRecoil/AK-47");
        Uzi = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/UziRecoil/Uzi");
        Colt = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/ColtM4Recoil/ColtM4");
        SMAW = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/SMAWRecoil/SMAW");
        Grenade = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/Grenade");

        if (isTDM)
        {
            tdmManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TDMManager>();
        }

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth >= maxHealth) //health value never goes beyond maximum set health
        {
            currentHealth = maxHealth;
        }
        //Controller Logic
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); //check if player is currently touching the ground

        if (!isDead)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z; //general movement logic

            controller.Move(move * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity); //jumping logic
            }

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2;
            }

            velocity.y += gravity * Time.deltaTime; //player gravity is coded. NOT from built-in physics
            controller.Move(velocity * Time.deltaTime);

            //Door Interaction for survival gamemode
            if (Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out RaycastHit hit, doorCheckDistance, doorLayer))
            {
                DoorController door = hit.transform.GetComponentInChildren<DoorController>();
                    if (door != null)
                    {
                        isPointingAtDoor = true;
                    }
                    else
                    {
                        isPointingAtDoor = false;
                    }

                if (isPointingAtDoor && Input.GetKeyDown(KeyCode.E))
                {
                    door.isOpen = !door.isOpen;
                }
            }
        }
        else if (isDead && isTDM) //if TDM, you are able to respawn
        {
            if (Input.GetButtonDown("Jump"))
            {
                Respawn();
            }
        }

    }


    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log("You just lost " + damage + " Health Points you dumbass");

        if (currentHealth <= 0 && !isDead)
        {
            Death();
        }
    }

    void Death()
    {
        isDead = true;

        if (isTDM)
        {
            tdmManager.playerDeathCount++;
            tdmManager.enemyScore += tdmManager.killValue; //adding score to enemy team if TDM
        }
    }

    void Respawn()
    {
       if (!tdmManager.matchOver)
        {
            gameObject.SetActive(false); //Temporarily deactivating player will interrupt any further transform updates until after player is repositioned
            int randSpawn = Random.Range(0, spawnPoints.Length); //Player spawns in randomly chosen friendly spawnpoint
            gameObject.transform.position = spawnPoints[randSpawn].transform.position;
            gameObject.SetActive(true);
            isDead = false;
            tdmManager.deathText.enabled = false;
            currentHealth = maxHealth;
            gunCam.SetActive(true);

            GameObject[] allGuns = { AK, Uzi, Colt }; //referencing all guns in player inventory and resetting ammo
            foreach (GameObject gun in allGuns)
            {
                Gun gunSettings = gun.GetComponent<Gun>();
                gunSettings.currentAmmo = gunSettings.maxAmmo;
                gunSettings.ammoPouch = 90;
            }

            SMAW.GetComponent<RocketLauncher>().ammoPouch = 5; //resetting grenade and rocket inventory
            Grenade.GetComponent<GrenadeThrower>().ammoPouch = 5;
        }
    }


    //ammo pickup logic

    public void AkPickup()
    {
        Gun AkAmmo = AK.GetComponent<Gun>();
        AkAmmo.ammoPouch += AkAmmo.maxAmmo;
    }

    public void UziPickup()
    {
        Gun uziAmmo = Uzi.GetComponent<Gun>();
        uziAmmo.ammoPouch += uziAmmo.maxAmmo;
    }

    public void RocketPickup()
    {
        RocketLauncher rockets = SMAW.GetComponent<RocketLauncher>();
        rockets.ammoPouch += rockets.maxAmmo;
    }

    public void M4Pickup()
    {
        Gun m4Ammo = Colt.GetComponent<Gun>();
        m4Ammo.ammoPouch += m4Ammo.maxAmmo;
    }

    public void GrenadePickup()
    {
        GrenadeThrower grenade = Grenade.GetComponent<GrenadeThrower>();
        grenade.ammoPouch += grenade.maxAmmo;
    }
}
