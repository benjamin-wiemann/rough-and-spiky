using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ProceduralMesh;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Visualizer : MonoBehaviour
{

    public enum CalculationMethod { CPU, GPU }

    const int maxResolution = 1024;

    [SerializeField, Range(8, maxResolution)]
    public int spectrumResolution = 512;

    [SerializeField, Range(8, maxResolution)]
    public int depth = 8;

    [SerializeField, Range(0.0005f, 0.01f)]
    float heightScale = 0.001f;

    [SerializeField, Range(20, 200)]
    int speed = 60;

    // Vertices per unit
    [SerializeField, Range(32, 4096)]
    int meshResolution = 512;

    Mesh mesh;

    // [SerializeField]
    // Material material;

    [SerializeField]
    CalculationMethod calculationMethod;
    
    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    AudioProcessor audioProcessor;

    GPUPointController gpuController;

    CPUPointController cpuController;

    float[] spectrum ;

    bool refreshNeeded = true;

    void Awake () {
		mesh = new Mesh {
			name = "Procedural Mesh"
		};
        GetComponent<MeshFilter>().mesh = mesh;
	}

    void OnEnable()
    {
        // if (Application.isPlaying)
        // {
            
        // }
    }

    void OnDisable()
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

    void OnValidate()
    {
        if (enabled)
        {
            refreshNeeded = true;
            // OnDisable();
            // OnEnable();
        }
    }

    void Update()
    {
        if( refreshNeeded )
        {
            GenerateMesh();
            spectrum = new float[spectrumResolution];
            audioProcessor.Initialize(spectrumResolution);            
            switch (calculationMethod)
            {
                case CalculationMethod.CPU:
                    cpuController = new CPUPointController( spectrumResolution, depth );
                    break;
                case CalculationMethod.GPU:
                    gpuController = new GPUPointController( spectrumResolution, depth);
                    break;
            }
            refreshNeeded = false;
        }
        switch (calculationMethod)
        {
            case CalculationMethod.CPU:
                cpuController.UpdatePointPosition( spectrumResolution, depth, speed, heightScale, spectrum, GetComponent<MeshRenderer>().material, mesh);
                break;
            case CalculationMethod.GPU:
                gpuController.UpdatePointPosition( computeShader, spectrumResolution, depth, speed, heightScale, spectrum, GetComponent<MeshRenderer>().material, mesh);
                break;
        }
    }

    void FixedUpdate()
    {
        spectrum = audioProcessor.GetSpectrumAudioSource();
    }


    void GenerateMesh () {
		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];

		MeshJob<SharedTriangleGrid>.ScheduleParallel(mesh, meshData, meshResolution, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
	}


}
