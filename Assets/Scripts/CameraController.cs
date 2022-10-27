using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField, Range(0.01f, 0.5f)]
    float moveSpeed = 0.2f;

    [SerializeField, Range(0.5f, 5f)]
    float turnSpeed = 4.0f;

    float rotX = -30f;

    float rotY = 180f;

    private bool rotating = false;

    private Vector3 moveVector;

    void Start()
    {
        rotX = transform.localEulerAngles.x;
        rotY = transform.localEulerAngles.y;
        moveVector = new Vector3(0, 0, 0);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire2"))
        {
            rotating = true;
        //     Cursor.lockState = CursorLockMode.Locked;
        //     Cursor.visible = false;            
        }
        if (Input.GetButton("Fire2"))
        {
            // get the mouse inputs
            rotY += Input.GetAxis("Mouse X") * turnSpeed;
            rotX += Input.GetAxis("Mouse Y") * turnSpeed;
            rotX = Mathf.Clamp(rotX, -90f, 90f);

            var yRad = rotY * 2 * Mathf.PI / 360;
            var xRad = rotX * 2 * Mathf.PI / 360;

            float h = (float)Input.GetAxisRaw("Horizontal");
            float v = (float)Input.GetAxisRaw("Vertical");
            moveVector.x = v * Mathf.Sin(yRad) * Mathf.Cos(xRad) + h * Mathf.Cos(yRad);
            moveVector.y = v * Mathf.Sin(xRad);
            moveVector.z = v * Mathf.Cos(yRad) * Mathf.Cos(xRad) - h * Mathf.Sin(yRad);
        }
        if (Input.GetButtonUp("Fire2"))
        {
        //     Cursor.lockState = CursorLockMode.None;
        //     Cursor.visible = true;
            rotating = false;

        }
    }

    void FixedUpdate()
    {
        if( rotating )
        {
            transform.position += moveSpeed * moveVector;
            transform.localEulerAngles = new Vector3(-rotX, rotY, 0);
        }
    }
}
