using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("Gun Settings: ")]
    public int damage = 10;
    public float range = 100;
    public float impactForce = 30;
    public float fireRate = 15;
    public int maxAmmo = 10;
    public int ammoPouch = 90;
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
    public LayerMask ignoreLayers;
    public LayerMask enemyLayer;
    public Animator recoilAnim;
    [Space()]



    private float nextFire = 0;
    private int currentAmmo;
    public bool isReloading = false;
    public bool isShooting = false;
    public GameObject muzzleLight;
    private Animator gunAnim;
    private PlayerHud playerHud;
    private WeaponSwitching weaponSwitching;
    private PlayerController playerController;

    private void Start()
    {
        currentAmmo = maxAmmo;
        playerHud = GameObject.FindGameObjectWithTag("hud").GetComponent<PlayerHud>();
        gunAnim = gameObject.GetComponent<Animator>();
        playerController = gameObject.GetComponentInParent<PlayerController>();
    }

    private void OnEnable()
    {
        isReloading = false;
        isShooting = false;
        weaponSwitching = gameObject.GetComponentInParent<WeaponSwitching>();
        gunCam.SetActive(true);
    }


    // Update is called once per frame
    void Update()
    {
        if (playerController.isDead)
        {
            gunCam.SetActive(false);
            return;
        }

        gunAnim.SetBool("isReloading", isReloading);
        weaponSwitching.isReloading = isReloading;
        weaponSwitching.isScoped = isScoped;

        if (isReloading)
        {
            return;
        }

        playerHud.ammoPouch = ammoPouch;
        playerHud.ammo = currentAmmo;
        recoilAnim.SetBool("isShooting", isShooting);


        if (Input.GetKeyDown(KeyCode.R) && ammoPouch > 0 && currentAmmo < maxAmmo || currentAmmo <= 0 && ammoPouch > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        gunAnim.SetBool("isScoped", isScoped);

        if (Input.GetButton("Fire2"))
        {
            isScoped = true;
        }

        else
        {
            isScoped = false;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextFire && currentAmmo > 0)
        {
            nextFire = Time.time + 1 / fireRate;
            Shoot();
            isShooting = true;
        }

        if (Input.GetButtonUp("Fire1") || currentAmmo <= 0)
        {
            isShooting = false;
        }

    }

    void Shoot()
    {
        muzzleFlash.Play();
        StartCoroutine(muzzleLighting());
        currentAmmo--;
        Vector3 shootDirection = fpsCam.transform.forward;

        if (!isScoped)
        {
            shootDirection.x += Random.Range(-recoilRotation, recoilRotation);
            shootDirection.y += Random.Range(-recoilRotation, recoilRotation);
            shootDirection.z += Random.Range(0, recoilRotation);
        }

        RaycastHit hit;

       if (Physics.Raycast(fpsCam.transform.position, shootDirection, out hit, range, ~ignoreLayers))
        {

            ZombieController zombie = hit.transform.GetComponent<ZombieController>();
            if (zombie != null)
            {
                zombie.TakeDamage(damage);
                GameObject bodyHit = Instantiate(bodyHitFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bodyHit, 1);
            }

            HumanController human = hit.transform.GetComponent<HumanController>();
            if (human != null)
            {
                human.TakeDamage(damage);
                GameObject bodyHit = Instantiate(bodyHitFX, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(bodyHit, 1);
            }

            BossController boss = hit.transform.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(damage);
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
