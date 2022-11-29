using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visualizer : MonoBehaviour
{

    public enum CalculationMethod { CPU, GPU }

    const int maxResolution = 1000;

    [SerializeField, Range(8, maxResolution)]
    public int resolution = 8;

    [SerializeField, Range(8, maxResolution)]
    public int depth = 8;

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

    GPUPointController gpuController;

    CPUPointController cpuController;

    float[] spectrum ;

    Transform[] points;

    void Start()
    {
        
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
                    cpuController = new CPUPointController( resolution, depth );
                    break;
                case CalculationMethod.GPU:
                    gpuController = new GPUPointController( maxResolution);
                    break;
            }
        }
    }

    void OnDisable()
    {
        if (Application.isPlaying)
        {
            if( gpuController != null)
            {
                gpuController.ReleaseBuffers();
            }
            if( cpuController != null)
            {
                cpuController.ReleaseBuffers();
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
                cpuController.UpdatePointPosition( resolution, depth, speed, heightScale, spectrum, material, mesh);
                break;
            case CalculationMethod.GPU:
                gpuController.UpdatePointPosition( computeShader, resolution, depth, speed, heightScale, spectrum, material, mesh);
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


}
