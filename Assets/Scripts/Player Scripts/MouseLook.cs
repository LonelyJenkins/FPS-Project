using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLook : MonoBehaviour
{
    public float mouseSensitivity = 100;
    public Transform playerBody;
    public float fallSpeed = 1;

    private float xRotation = 0;
    private bool hasLanded = false;//bool for coded death animation (ONLY FUNCTIONAL IN SURVIVAL GAME MODE)
    private PlayerController playerController;
    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerController = gameObject.GetComponentInParent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerController.isDead)

        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90, 90);

            transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
            playerBody.Rotate(Vector3.up * mouseX);
        }

        else
        {
                if (!hasLanded && !playerController.isTDM && !playerController.isChaos)
                transform.Translate(Vector3.down * fallSpeed * Time.deltaTime); //This acts as the motion for the death animation in survival mode
        }

    }

    private void OnTriggerEnter(Collider other)
    {//ground check to ensure death camera does not clip through floor. This acts as the death animation for survival mode
        if (other.CompareTag("Ground"))
        {
            hasLanded = true;
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
    }
}
