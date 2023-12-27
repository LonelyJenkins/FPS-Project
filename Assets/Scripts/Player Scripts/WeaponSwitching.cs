
using UnityEngine;

public class WeaponSwitching : MonoBehaviour
{
    public int currentWeapon = 0;
    public bool isReloading;
    public bool isScoped;

    private PlayerController playerController;

    // Start is called before the first frame update
    void Start()
    {
        SelectWeapon();
        playerController = gameObject.GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.isDead)
        {

            int previousWeapon = currentWeapon;

            if (Input.GetAxis("Mouse ScrollWheel") > 0 && !isReloading && !isScoped)
            {
                if (currentWeapon >= transform.childCount - 1)
                    currentWeapon = 0;
                else
                    currentWeapon++;
            }

            if (Input.GetAxis("Mouse ScrollWheel") < 0 && !isReloading && !isScoped)
            {
                if (currentWeapon <= 0)

                    currentWeapon = transform.childCount - 1;
                else

                    currentWeapon--;
            }

            if (previousWeapon != currentWeapon)
            {
                SelectWeapon();
            }
        }

    }

    void SelectWeapon()
    {
        int i = 0;

        foreach (Transform weapon in transform)
        {
            if (i == currentWeapon)
                weapon.gameObject.SetActive(true);

            else
                weapon.gameObject.SetActive(false);
            i++;
        }

    }

}
