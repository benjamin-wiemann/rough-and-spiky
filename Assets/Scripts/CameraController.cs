using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField, Range(0.01f, 0.5f)]
    float moveSpeed = 0.1f;

    [SerializeField, Range(1f, 10f)]
    float turnSpeed = 4.0f;

    [SerializeField]
    Transform player;
    
    [SerializeField]
    float rotX = -45f;

    [SerializeField]
    float rotY = 36f;
    
    private Vector3 moveVector;

    void Start()
    {
        moveVector = new Vector3(0, 0, 0);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            if(Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }   
            else
            {
                Cursor.lockState = CursorLockMode.Locked;   
                Cursor.visible = false; 
            }
        }
        if( Cursor.lockState == CursorLockMode.Locked)
        {
            // get the mouse inputs
            rotY += Input.GetAxis("Mouse X") * turnSpeed;
            rotX += Input.GetAxis("Mouse Y") * turnSpeed;
            rotX = Mathf.Clamp(rotX, -90f, 90f);

            var yRad = rotY * 2 * Mathf.PI / 360;
            var xRad = rotX * 2 * Mathf.PI / 360;
        
            float h = ( float ) Input.GetAxisRaw("Horizontal");
            float v = ( float ) Input.GetAxisRaw("Vertical");
            moveVector.x = v * Mathf.Sin( yRad )*Mathf.Cos( xRad ) + h * Mathf.Cos( yRad ); 
            moveVector.y = v * Mathf.Sin( xRad );
            moveVector.z = v * Mathf.Cos( yRad )*Mathf.Cos( xRad ) - h * Mathf.Sin( yRad );
        }       
    }

    void FixedUpdate()
    {
        player.position += moveSpeed * moveVector;
        transform.localEulerAngles = new Vector3(-rotX, 0, 0);        
        // rotate the camera
        player.localEulerAngles = new Vector3(0, rotY, 0);
    }
}
