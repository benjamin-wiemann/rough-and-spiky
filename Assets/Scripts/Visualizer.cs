using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour
{

    public enum CalculationMethod { CPU, GPU, Burst }

    const int maxResolution = 1000;

    [SerializeField, Range(8, maxResolution)]
    public int resolution = 8;

    [SerializeField, Range(8, maxResolution)]
    public int depth = 8;

    // [SerializeField, Range(1f, 20f)]
    // float heightScale = 5f;

    [SerializeField, Range(0.001f, 0.1f)]
    float heightScale = 0.01f;

    [SerializeField, Range(20, 200)]
    int speed = 60;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    [SerializeField]
    Transform pointPrefab;

    [SerializeField]
    CalculationMethod calculationMethod;
    
    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    AudioProcessor audioProcessor;

    GPUController gpuController;

    float[] spectrum ;

    Transform[] points;

    void Start()
    {
        gpuController = new GPUController( maxResolution);
    } 


    void OnEnable()
    {
        if (Application.isPlaying)
        {
            spectrum = new float[resolution];
            audioProcessor.Initialize(resolution);
            switch (calculationMethod)
            {
                case CalculationMethod.CPU:
                    InitPointCPU();
                    break;
                case CalculationMethod.GPU:
                    
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
                    if (points != null)
                    {
                        for (int i = 0; i < points.Length; i++)
                        {
                            Destroy(points[i].gameObject);
                        }
                        points = null;
                    }
                    break;
                case CalculationMethod.GPU:
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

    void Update()
    {
        switch (calculationMethod)
        {
            case CalculationMethod.CPU:
                UpdatePointPositionCPU();
                break;
            case CalculationMethod.GPU:
                gpuController.UpdatePointPosition( computeShader, resolution, depth, speed, heightScale, spectrum, material, mesh);
                break;
            case CalculationMethod.Burst:
                break;
        }
    }

    void FixedUpdate()
    {
        spectrum = audioProcessor.GetSpectrumAudioSource();
    }

    void InitPointCPU()
    {
        float step = 2f / resolution;
        var scale = Vector3.one * step;
        points = new Transform[resolution * depth];
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i] = Instantiate(pointPrefab);
            point.localScale = scale;
            point.SetParent(transform, false);
        }
        audioProcessor.Initialize(resolution);
    }



    void UpdatePointPositionCPU()
    {
        float step = 2f / resolution;
        float z = 0.5f * step - 1f;

        for (int i = 0, j = 0; i < points.Length; i++, j++)
        {
            if (j == resolution)
            {
                j = 0;
                z += step;
            }

            float x = (j + 0.5f) * step - 1f;
            float y;
            if (i >= points.Length - resolution)
            {
                y = heightScale * 20 * Mathf.Log(spectrum[j], 10);
            }
            else
            {
                float timeSensitiveFactor = i + Time.deltaTime * speed;
                int upperFactor = (int) Mathf.Ceil(timeSensitiveFactor);
                int lowerFactor = (int) Mathf.Floor(timeSensitiveFactor);
                float delta = timeSensitiveFactor - lowerFactor;
                if( i + upperFactor * resolution <= points.Length )
                {
                    y = points[i + upperFactor * resolution].localPosition.y * delta + points[i + lowerFactor * resolution].localPosition.y * (1 - delta);
                }
                else
                {   
                    y = points[i + resolution].localPosition.y;
                }
            }

            points[i].localPosition = new Vector3(x, y, z); 
        }

    }
}
