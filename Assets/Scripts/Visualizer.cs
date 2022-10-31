using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour
{

    public enum CalculationMethod { CPU, GPU, Burst }

    const int maxResolution = 1000;

    [SerializeField, Range(8, maxResolution)]
    public int xResolution = 8;

    [SerializeField, Range(8, maxResolution)]
    public int zResolution = 8;

    // [SerializeField, Range(1f, 20f)]
    // float heightScale = 5f;

    [SerializeField, Range(0.001f, 0.1f)]
    float heightScale = 0.01f;

    [SerializeField]
    float minHeight = 0.0001f;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    [SerializeField]
    Transform pointPrefab;

    [SerializeField]
    CalculationMethod calculationMethod;

    [SerializeField]
    AudioProcessor audioProcessor;

    ComputeBuffer positionsBuffer;

    Transform[] points;

    // Event emitting on resolution change
    public delegate void resChangeDelegate( int resolution );

    public static event resChangeDelegate resChangeEvent;

    void OnEnable()
    {
        if (Application.isPlaying)
        {
            switch (calculationMethod)
            {
                case CalculationMethod.CPU:
                    InitPointCPU();
                    break;
                case CalculationMethod.GPU:
                    positionsBuffer = new ComputeBuffer(xResolution * zResolution, 3);
                    break;
                case CalculationMethod.Burst:
                    break;
            }
        }
    }

    void OnDisable()
    {
        if (Application.isPlaying)
        {
            switch (calculationMethod)
            {
                case CalculationMethod.CPU:
                    if(points != null)
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            Destroy(points[i].gameObject);
                        }
                        points = null;
                    }          
                    break;
                case CalculationMethod.GPU:
                    positionsBuffer.Release();
                    positionsBuffer = null;
                    break;
                case CalculationMethod.Burst:
                    break;
            }
        }

    }

    void OnValidate()
    {
        if (enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    void FixedUpdate()
    {
        switch (calculationMethod)
        {
            case CalculationMethod.CPU:
                UpdatePointPositionCPU();
                break;
            case CalculationMethod.GPU:
                var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / xResolution));
                Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, xResolution * zResolution);
                break;
            case CalculationMethod.Burst:
                break;
        }
    }

    void InitPointCPU()
    {
        float step = 2f / xResolution;
        var scale = Vector3.one * step;
        points = new Transform[xResolution * zResolution];
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i] = Instantiate(pointPrefab);
            point.localScale = scale;
            point.SetParent(transform, false);
        }
    }

    void UpdatePointPositionCPU()
    {
        float step = 2f / xResolution;
        float z = 0.5f * step - 1f;
        
        float[] spectrum = audioProcessor.GetSpectrumAudioSource();
        for (int i = 0, j = 0; i < points.Length; i++, j++)
        {
            if (j == xResolution)
            {
                j = 0;
                z += step;
            }

            float x = (j + 0.5f) * step - 1f;
            float y;
            if (i >= points.Length - xResolution)
            {
                y = heightScale * 20 * Mathf.Log( spectrum[j], 10);
                
            }
            else
            {
                y = points[i+xResolution].localPosition.y;
            }
            
            points[i].localPosition = new Vector3(x, y, z);
        }

    }
}
