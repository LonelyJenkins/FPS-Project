using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadeThrower : MonoBehaviour
{
    public float throwForce = 40;
    public float reloadTime = 1;
    public int maxAmmo = 1;
    public int ammoPouch = 4;
    public float animTime = 0.1f;

    public GameObject grenadePrefab;
    public GameObject launchPoint;
    public GameObject gunCam;

    private int currentAmmo;
    private bool isReloading = false;
    private PlayerHud playerHud;
    private Animator grenadeAnim;
    private WeaponSwitching weaponSwitching;
    private PlayerController playerController;
    // Start is called before the first frame update
    void Start()
    {
        currentAmmo = maxAmmo;
        playerHud = GameObject.FindGameObjectWithTag("hud").GetComponent<PlayerHud>();
        playerController = gameObject.GetComponentInParent<PlayerController>();
        grenadeAnim = gameObject.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        isReloading = false;
        weaponSwitching = gameObject.GetComponentInParent<WeaponSwitching>();
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

        if (isReloading)
        {
            return;
        }

        playerHud.ammoPouch = ammoPouch;
        playerHud.ammo = currentAmmo;

        if (currentAmmo <= 0 && ammoPouch > 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetMouseButtonDown(0) && currentAmmo > 0)
        {
            ThrowGrenade();
        }

        if ((currentAmmo <= 0) && (ammoPouch <= 0))
        {
            MeshRenderer[] mesh = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshs in mesh)
                meshs.enabled = false;
        }
    }

    void ThrowGrenade()
    {
        grenadeAnim.SetTrigger("throwTrig");
        StartCoroutine(ThrowTime());
    }

    IEnumerator Reload()
    {
        isReloading = true;

        MeshRenderer[] mesh = gameObject.GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer meshs in mesh)
        meshs.enabled = false;

        yield return new WaitForSeconds(reloadTime);

        currentAmmo++;
        ammoPouch--;

        if (currentAmmo > 0)
        {
            foreach (MeshRenderer meshs in mesh)
                meshs.enabled = true;
        }
        isReloading = false;
    }

    IEnumerator ThrowTime()
    {
        yield return new WaitForSeconds(animTime);

        GameObject grenade = Instantiate(grenadePrefab, launchPoint.transform.position, Quaternion.identity);
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * throwForce, ForceMode.Impulse);

        currentAmmo--;
    }
}
