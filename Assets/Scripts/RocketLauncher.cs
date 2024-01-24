using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public int maxAmmo = 1;
    public int ammoPouch = 5;
    public float reloadTime = 1;
    public float recoilRotation = 5;
    public float scopeTime = 0.5f;
    public float scopedFOV = 2;
    private float normalFOV;

    public ParticleSystem muzzleFlash;
    public GameObject rocket;
    public GameObject muzzle;
    public Camera fpsCam;
    public GameObject gunCam;
    public Animator recoilAnim;
    public GameObject scopeOverlay;

    private int currentAmmo;
    private bool isReloading = false;
    private bool isScoped = false;
    private PlayerHud playerHud;
    private Animator gunAnim;
    private MouseLook mouseLook;
    private WeaponSwitching weaponSwitching;
    private PlayerController playerController;

    private void Start()
    {
        currentAmmo = maxAmmo;
        playerHud = GameObject.FindGameObjectWithTag("hud").GetComponent<PlayerHud>();
        playerController = gameObject.GetComponentInParent<PlayerController>();
        gunAnim = gameObject.GetComponent<Animator>();
        mouseLook = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<MouseLook>();
        normalFOV = fpsCam.fieldOfView;
    }

    private void OnEnable()
    {
        isReloading = false;
        weaponSwitching = gameObject.GetComponentInParent<WeaponSwitching>();
        gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0); //Resets object rotation on enable. Addresses bug where rotation is frozen if player dies mid-animation

    }

    // Update is called once per frame
    void Update()
    {
        if (playerController.isDead)
        {
            gunCam.SetActive(false);
            return;
        }

        weaponSwitching.isReloading = isReloading;
        weaponSwitching.isScoped = isScoped;
        gunAnim.SetBool("isReloading", isReloading);

        if (isReloading)
        {
            return;
        }

        playerHud.ammoPouch = ammoPouch;
        playerHud.ammo = currentAmmo;

        if (currentAmmo <= 0 && ammoPouch > 0 && !isScoped)
        {
            StartCoroutine(Reload());
            return;
        }

        gunAnim.SetBool("isScoped", isScoped);

        if (Input.GetButton("Fire2"))
        {
            isScoped = true;
            StartCoroutine(OnScoped());
        }

        else
        {
            isScoped = false;
            OnUnScoped();
            fpsCam.fieldOfView = normalFOV;
        }

        if (Input.GetButtonDown("Fire1") && currentAmmo > 0)
        {
            Shoot();
            recoilAnim.SetTrigger("recoil");
        }
    }


    void Shoot()
    {
        muzzleFlash.Play();


        Instantiate(rocket, muzzle.transform.position, muzzle.transform.rotation);

        currentAmmo--;
    }

    IEnumerator Reload()
    {
        isReloading = true;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo++;
        ammoPouch--;

        isReloading = false;
    }

    IEnumerator OnScoped()
    {
        yield return new WaitForSeconds(scopeTime);
        {
            if(isScoped)
            {
                scopeOverlay.SetActive(true);
                gunCam.SetActive(false);

                fpsCam.fieldOfView = scopedFOV;
            }
        }
    }

    void OnUnScoped()
    {
        scopeOverlay.SetActive(false);
        gunCam.SetActive(true);

    }
}
