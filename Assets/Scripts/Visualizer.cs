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


    // Number of samples in a spectrum
    [SerializeField, Range(8, maxResolution)]
    public int spectrumResolution = 512;
    int spectrumResolutionOld;

    // Number of spectrums in the spectrogram
    [SerializeField, Range(8, 8192)]
    int spectrogramDepth = 8;
    int spectrogramDepthOld;

    [SerializeField, Range(0.0005f, 0.01f)]
    float heightScale = 0.001f;

    [SerializeField, Range(20, 200)]
    int speed = 60;

    // Vertices per unit in mesh
    [SerializeField, Range(32, 4096)]
    int meshResolution = 512;
    int meshResolutionOld;

    // Size of mesh in X direction
    [SerializeField, Range(1, 10)]
    float meshX = 1;
    float meshXOld;

    // Size of mesh in Y direction
    [SerializeField, Range(1, 100)]
    float meshZ = 1;
    float meshZOld;

    Mesh mesh;

    [SerializeField]
    CalculationMethod calculationMethod;
    
    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    AudioProcessor audioProcessor;

    GPUPointController gpuController;

    CPUPointController cpuController;

    float[] spectrum ;

    bool spectrogramChanged = true;

    bool meshChanged = true;

    void Awake () {
		mesh = new Mesh {
			name = "Procedural Mesh"
		};
        GetComponent<MeshFilter>().mesh = mesh;

        spectrogramDepthOld = spectrogramDepth;
        spectrumResolutionOld = spectrumResolution;
        meshResolutionOld = meshResolution;
        meshXOld = meshX;
        meshZOld = meshZ;

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
            if( spectrogramDepthOld != spectrogramDepth 
            || spectrumResolutionOld != spectrumResolution)
            {
                spectrogramChanged = true;
                spectrogramDepthOld = spectrogramDepth;
                spectrumResolutionOld = spectrumResolution;
            }
            if( meshResolutionOld != meshResolution 
            || meshXOld != meshX 
            || meshZOld != meshZ )
            {
                meshChanged = true;
                meshResolutionOld = meshResolution;
                meshXOld = meshX;
                meshZOld = meshZ;
            }
        }
    }

    void Update()
    {
        if( spectrogramChanged )
        {
            spectrum = new float[spectrumResolution];
            audioProcessor.Initialize(spectrumResolution);            
            switch (calculationMethod)
            {
                case CalculationMethod.CPU:
                    cpuController = new CPUPointController( spectrumResolution, spectrogramDepth );
                    break;
                case CalculationMethod.GPU:
                    gpuController = new GPUPointController( spectrumResolution, spectrogramDepth);
                    break;
            }
            spectrogramChanged = false;
        }
        if( meshChanged)
        {
            GenerateMesh();
            meshChanged = false;
        }
        switch (calculationMethod)
        {
            case CalculationMethod.CPU:
                cpuController.UpdatePointPosition( spectrumResolution, spectrogramDepth, speed, heightScale, spectrum, GetComponent<MeshRenderer>().material, mesh);
                break;
            case CalculationMethod.GPU:
                gpuController.UpdatePointPosition( computeShader, spectrumResolution, spectrogramDepth, speed, heightScale, spectrum, GetComponent<MeshRenderer>().material, mesh);
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

		MeshJob<SharedTriangleGrid>.ScheduleParallel(mesh, meshData, meshResolution, meshX, meshZ, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
	}


}
