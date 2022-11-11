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
    AudioProcessor audioProcessor;

    [SerializeField]
    ComputeShader computeShader;

    ComputeBuffer freqBandsBuffer;

    ComputeBuffer positionsBufferA;

    ComputeBuffer positionsBufferB;

    float cumulatedDeltaTime = 0;

    enum ReadFromBuffer{
        A,
        B
    }

    ReadFromBuffer readFromBuffer = ReadFromBuffer.A;

    float[] spectrum ;

    static readonly int
		positionsId = Shader.PropertyToID("_Positions"),
        prevPositionsId = Shader.PropertyToID("_PrevPositions"),
        frequencyBandsId = Shader.PropertyToID("_FrequencyBands"),
		resolutionId = Shader.PropertyToID("_Resolution"),
		stepId = Shader.PropertyToID("_Step"),
        indexOffsetId = Shader.PropertyToID("_IndexOffset"),
        depthId = Shader.PropertyToID("_Depth"),
        heightId = Shader.PropertyToID("_HeightScale");



    Transform[] points;

    void Start()
    {
        positionsBufferA = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
        positionsBufferB = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
        freqBandsBuffer = new ComputeBuffer( maxResolution, 4);
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
                    // positionsBufferA = new ComputeBuffer(resolution * depth, 3 * 4);
                    // positionsBufferB = new ComputeBuffer(resolution * depth, 3 * 4);
                    // freqBandsBuffer = new ComputeBuffer(resolution, 4);
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
                    // if ( positionsBufferA != null)
                    // {
                    //     positionsBufferA.Release();
                    //     positionsBufferA = null;
                    // }
                    // if ( positionsBufferB != null)
                    // {
                    //     positionsBufferB.Release();
                    //     positionsBufferB = null;
                    // }
                    // if ( freqBandsBuffer != null )
                    // {
                    //     freqBandsBuffer.Release();
                    //     freqBandsBuffer = null;                    
                    // }
                    // break;
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
                UpdatePointPositionGPU();
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

    void UpdatePointPositionGPU()
    {
        
        int kernelHandle = computeShader.FindKernel("SpectrumVisualizer");

        float step = 2f / resolution;
        cumulatedDeltaTime += speed * Time.deltaTime;
        
		computeShader.SetInt(resolutionId, resolution);
		computeShader.SetFloat(stepId, step);
		computeShader.SetInt(indexOffsetId, Mathf.FloorToInt(cumulatedDeltaTime));
		computeShader.SetInt(depthId, depth);
		computeShader.SetFloat(heightId, heightScale);

        freqBandsBuffer.SetData(spectrum);

        if( readFromBuffer == ReadFromBuffer.A )
        {
            computeShader.SetBuffer(kernelHandle, prevPositionsId, positionsBufferA);
            computeShader.SetBuffer(kernelHandle, positionsId, positionsBufferB);
        }
        else
        {
            computeShader.SetBuffer(kernelHandle, prevPositionsId, positionsBufferB);
            computeShader.SetBuffer(kernelHandle, positionsId, positionsBufferA);
        }
        computeShader.SetBuffer(kernelHandle, frequencyBandsId, freqBandsBuffer);

        int groupsX = Mathf.CeilToInt( resolution / 8f );
        int groupsY = Mathf.CeilToInt( depth / 8f);
		computeShader.Dispatch(kernelHandle, groupsX, groupsY, 1);
        
        if( readFromBuffer == ReadFromBuffer.A )
        {
            material.SetBuffer(positionsId, positionsBufferB);
            readFromBuffer = ReadFromBuffer.B;
        }
        else
        {
            material.SetBuffer(positionsId, positionsBufferA);
            readFromBuffer = ReadFromBuffer.A;
        }
        
		material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * depth);
        
        if( cumulatedDeltaTime - Mathf.FloorToInt(speed * Time.deltaTime) >= 1)
        {
            cumulatedDeltaTime -= Mathf.FloorToInt(cumulatedDeltaTime);             
        }
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
