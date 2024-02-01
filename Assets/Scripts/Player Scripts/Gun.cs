using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings: ")]
    public int damage = 10;
    public float range = 100;
    public float impactForce = 3000;
    public float fireRate = 15;
    public int maxAmmo = 10;
    public int ammoPouch = 90;
    public int currentAmmo;
    public float reloadTime = 1;
    public bool isScoped = false;
    public float recoilRotation = 0.1f;
    [Space()]

    [Header("Objects & Layermasks")]
    public Camera fpsCam;
    public GameObject gunCam;
    public ParticleSystem muzzleFlash;
    public GameObject impactFX;
    public GameObject bodyHitFX;
    public HitMarker hitMarker;
    public LayerMask ignoreLayers;
    public LayerMask enemyLayer;
    public Animator recoilAnim;
    [Space()]



    private float nextFire = 0;
    public bool isReloading = false;
    public bool isShooting = false;
    public GameObject muzzleLight;
    private Animator gunAnim;
    private PlayerHud playerHud;
    private WeaponSwitching weaponSwitching;
    private PlayerController playerController;
    private Light lightFlash;
    private float flashDuration;
    private float lightStartTime;

    private void Start()
    {
        currentAmmo = maxAmmo;
        playerHud = GameObject.FindGameObjectWithTag("hud").GetComponent<PlayerHud>();
        gunAnim = gameObject.GetComponent<Animator>();
        playerController = gameObject.GetComponentInParent<PlayerController>();
        flashDuration = (muzzleFlash.main.duration / 2);
        lightFlash = muzzleLight.GetComponent<Light>();

    }

    private void OnEnable()
    {
        isReloading = false;
        isShooting = false;
        weaponSwitching = gameObject.GetComponentInParent<WeaponSwitching>();
        gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0); //Resets object rotation on enable. Addresses bug where rotation is frozen if player dies mid-animation
        gunCam.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        if (playerController.isDead)
        {
            gunCam.SetActive(false); //if player dies, weapons will no longer be visible/active
            return;
        }

        gunAnim.SetBool("isReloading", isReloading);//sets animation parameters
        weaponSwitching.isReloading = isReloading;//addresses bools in weapon switcher that prohibits switching if reloading/ADS
        weaponSwitching.isScoped = isScoped;

        if (isReloading)
        {
            return;//prohibits further weapon logic until reloading returns false
        }

        playerHud.ammoPouch = ammoPouch;//references player hud for ammo data
        playerHud.ammo = currentAmmo;
        recoilAnim.SetBool("isShooting", isShooting);//Sets shooting animation if player is actively shooting


        if (Input.GetKeyDown(KeyCode.R) && ammoPouch > 0 && currentAmmo < maxAmmo || currentAmmo <= 0 && ammoPouch > 0)
        {
            //reload logic
            StartCoroutine(Reload());
            return;
        }

        gunAnim.SetBool("isScoped", isScoped);

        if (Input.GetButton("Fire2"))
        {
            //ADS logic
            isScoped = true;
        }

        else
        {
            isScoped = false;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextFire && currentAmmo > 0) //shooting logic
        {
            nextFire = Time.time + 1 / fireRate;
            Shoot();
            isShooting = true;//setting shoot animation
        }

        if (Time.time >= lightStartTime + flashDuration) //logic for resetting muzzleFlash lighting
        {
            muzzleLight.SetActive(false);
        }

        if (Input.GetButtonUp("Fire1") || currentAmmo <= 0)
        {
            isShooting = false;
        }

    }

    void Shoot()
    {
        muzzleFlash.Play(); //logic for muzzle flash vfx
        lightStartTime = Time.time;
        lightFlash.range = (Random.Range(3, 9));
        muzzleLight.SetActive(true);
        //further shooting logic begins
        currentAmmo--;
        Vector3 shootDirection = fpsCam.transform.forward;

        if (!isScoped)
        {
            shootDirection.x += Random.Range(-recoilRotation, recoilRotation); //if NOT ADS, gun is less accurate
            shootDirection.y += Random.Range(-recoilRotation, recoilRotation);
            shootDirection.z += Random.Range(0, recoilRotation);
        }

        RaycastHit hit;

       if (Physics.Raycast(fpsCam.transform.position, shootDirection, out hit, range, ~ignoreLayers)) //checks for type of target is hit, if any
        {

            ZombieController zombie = hit.transform.GetComponent<ZombieController>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage, true);
                GameObject bodyHit = Instantiate(bodyHitFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bodyHit, 1);
                hitMarker.IndicateHit();
            }

            HumanController human = hit.transform.GetComponent<HumanController>();
            if (human != null)
            {
                human.TakeDamage(damage, true);
                GameObject bodyHit = Instantiate(bodyHitFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bodyHit, 1);
                hitMarker.IndicateHit();
            }

            BossController boss = hit.transform.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damage, true);
                GameObject bodyHit = Instantiate(bodyHitFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bodyHit, 1);
                hitMarker.IndicateHit();

            }

            if (hit.rigidbody != null)
            {
                hit.rigidbody.AddForce(-hit.normal * impactForce);
            }

            GameObject impactObject = Instantiate(impactFX, hit.point, Quaternion.LookRotation(hit.normal));
            Destroy(impactObject, 1);

        }
    }

    IEnumerator Reload() //coroutine set for reload to prevent other logic from being performed until reload time has elapsed
    {
        isReloading = true;
        recoilAnim.enabled = false;
        isShooting = false;

        yield return new WaitForSeconds(reloadTime);

        int ammoToReload = maxAmmo - currentAmmo;

       if (ammoToReload <= ammoPouch)
        {
            currentAmmo += ammoToReload;
            ammoPouch -= ammoToReload;
        }

       else if (ammoToReload > ammoPouch)
        {
            currentAmmo += ammoPouch;
            ammoPouch = 0;
        }

        isReloading = false;
        recoilAnim.enabled = true;
    }

    IEnumerator muzzleLighting()
    {
        Light light = muzzleLight.GetComponent<Light>();
        light.range = (Random.Range(3, 9));
        muzzleLight.SetActive(true);
        yield return new WaitForSeconds(muzzleFlash.main.duration/2.5f);
        muzzleLight.SetActive(false);
    }
}
