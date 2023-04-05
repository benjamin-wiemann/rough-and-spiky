using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightMovement : MonoBehaviour
{

    [SerializeField, Range(0.1f, 1f)]
    float lightMovementSpeed = 0.5f;
    
    [SerializeField, Range(5f, -5f)]
    float offset = 0f;

    [HideInInspector]
    public float MeshZ{ get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 pos = GetComponent<Transform>().position;
        GetComponent<Transform>().position = new Vector3(pos.x, pos.y, Mathf.PerlinNoise(Time.time * lightMovementSpeed, 0) * MeshZ + offset);
    }
}
