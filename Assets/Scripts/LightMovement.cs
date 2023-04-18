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

    void Update()
    {
        Vector3 pos = GetComponent<Transform>().position;
        float z = Mathf.PerlinNoise(Time.time * lightMovementSpeed, 0) * MeshZ + offset;
        GetComponent<Transform>().position = new Vector3(pos.x, pos.y, z);
    }
}
