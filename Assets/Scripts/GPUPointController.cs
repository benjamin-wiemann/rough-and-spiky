using UnityEngine;

using Unity.Mathematics;

public abstract class GPUPointController
{


    protected float[] debugSpectrogram;

    protected Material material;

    protected enum ReadFromBuffer
    {
        A,
        B
    }

    protected ReadFromBuffer readFromBuffer = ReadFromBuffer.A;

    protected float cumulatedDeltaTime = 0;

    protected static readonly int
    spectrogramId = Shader.PropertyToID("_Spectrogram"),
    prevSpectrogramId = Shader.PropertyToID("_PrevSpectrogram"),
    resolutionId = Shader.PropertyToID("_Resolution"),
    indexOffsetId = Shader.PropertyToID("_IndexOffset"),
    depthId = Shader.PropertyToID("_Depth"),
    heightId = Shader.PropertyToID("_HeightScale"),
    meshXId = Shader.PropertyToID("_MeshX"),
    meshZId = Shader.PropertyToID("_MeshZ");

    public GPUPointController( Material material, int maxResolution, int depth )
    {
        this.material = material;
    }

    ~GPUPointController()
    {
        ReleaseBuffers();
    }

    public abstract void ReleaseBuffers();
    

    public void UpdatePointPosition(
        ComputeShader computeShader,
        int resolution,
        int depth, 
        float spectrumShiftTime, 
        float heightScale, 
        float meshX,
        float meshZ,
        float[] spectrum, 
        Mesh mesh,
        bool debug)
    {
        if( !debug )
        {
            int kernelHandle = computeShader.FindKernel("SpectrumVisualizer");

            cumulatedDeltaTime += Time.deltaTime;
            int indexOffset = Mathf.FloorToInt(cumulatedDeltaTime / spectrumShiftTime);

            computeShader.SetInt(resolutionId, resolution);
            computeShader.SetInt(indexOffsetId, indexOffset);
            computeShader.SetInt(depthId, depth);
            SendSpectrumToShader(computeShader, depth, spectrumShiftTime, spectrum, kernelHandle, indexOffset);

            int groupsX = Mathf.CeilToInt(resolution / 8f);
            int groupsY = Mathf.CeilToInt(depth / 8f);
            computeShader.Dispatch(kernelHandle, groupsX, groupsY, 1);
            BindToMaterial();
            material.SetInteger(resolutionId, resolution);
            material.SetInteger(depthId, depth);
            material.SetFloat(heightId, heightScale);
            material.SetFloat(meshXId, meshX);
            material.SetFloat(meshZId, meshZ);

        }
        else
        {
            GenerateDebugSpectrogram(resolution, depth);          
            
            material.SetInteger(resolutionId, resolution);
            material.SetInteger(depthId, depth);
            material.SetFloat(heightId, heightScale);
            material.SetFloat(meshXId, meshX);
            material.SetFloat(meshZId, meshZ);

        }
    }

    protected abstract void SetDebugSpectrogram();


    protected abstract void SendSpectrumToShader(ComputeShader computeShader, int depth, float spectrumShiftTime, float[] spectrum, int kernelHandle, int indexOffset);
    

    protected abstract void BindToMaterial();

    void GenerateDebugSpectrogram(int resolution, int depth)
    {
        bool init = false;
        if( debugSpectrogram == null)
        {
            debugSpectrogram = new float[(depth +1) * resolution];
            init = true;
        }
        if( debugSpectrogram.Length != (depth + 1) * resolution)
        {
            debugSpectrogram = new float[(depth + 1) * resolution];
            init = true;
        }
        if( init )
        {
            for( int i = 0; i <= depth; i++ )
            {
                for( int j = 0; j < resolution; j++ )
                {
                    debugSpectrogram[j + i * resolution] = 100f *(Mathf.Sin(2f * (float) i / (float) depth * Mathf.PI) + Mathf.Sin(2f * (float) j / (float) resolution * Mathf.PI));
                }
            }
        }
        SetDebugSpectrogram();
    }
}