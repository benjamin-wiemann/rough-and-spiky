using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ProceduralMesh;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Visualizer : MonoBehaviour
{

    public enum ShaderType { Texture, ComputeBuffer }

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

    [SerializeField, Range(0.005f, 0.1f)]
    float spectrumShiftTime = 0.016f;

    // Vertices per unit in mesh
    [SerializeField, Range(8, 1024)]
    int meshResolution = 128;
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
    ShaderType shaderType;
    
    [SerializeField]
    ComputeShader computeShader;

    [SerializeField]
    AudioProcessor audioProcessor;

    [SerializeField]
    bool debugShader = false;

    GPUPointController gpuController;

    float[] spectrum ;

    bool spectrogramChanged = true;

    bool meshChanged = true;

    Vector3[] vertices, normals;

	Vector4[] tangents;

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


    void OnDisable()
    {

            if( gpuController != null)
            {
                gpuController.ReleaseBuffers();
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
            switch( shaderType )      
            {
                case ShaderType.ComputeBuffer:
                    gpuController = new SpectrumBufferController(  GetComponent<MeshRenderer>().material, spectrumResolution, spectrogramDepth);
                    break;
                case ShaderType.Texture:
                    gpuController = new SpectrumTextureController(  GetComponent<MeshRenderer>().material, spectrumResolution, spectrogramDepth);
                    break;
            } 
            spectrogramChanged = false;
        }
        if( meshChanged)
        {
            GenerateMesh();
            meshChanged = false;

        }
        gpuController.UpdatePointPosition( 
            computeShader, 
            spectrumResolution, 
            spectrogramDepth, 
            spectrumShiftTime, 
            heightScale, 
            meshX, 
            meshZ, 
            spectrum,            
            mesh,
            debugShader);
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
