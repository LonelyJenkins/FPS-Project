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
    [Space()]

    [Header("GameMode Settings")]
    public bool isSurvival = false;
    public bool isTDM = false;

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
        tdmManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<TDMManager>();
        AK = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/AkRecoil/AK-47");
        Uzi = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/UziRecoil/Uzi");
        Colt = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/ColtM4Recoil/ColtM4");
        SMAW = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/SMAWRecoil/SMAW");
        Grenade = GameObject.Find("Player/PlayerCharacter/Main Camera/WeaponSlot/Grenade");

        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth >= maxHealth)
        {
            currentHealth = maxHealth;
        }
        //Controller Logic
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }

        if (!isDead)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            controller.Move(move * speed * Time.deltaTime);

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
            }

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            //Door Interaction
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
        else if (isDead && isTDM)
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
            tdmManager.enemyScore += tdmManager.killValue;
        }
    }

    void Respawn()
    {
        isDead = false;
        currentHealth = maxHealth;
        //assign new spawnpoint through gamemanager
        //reset all ammunition to starting values
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
