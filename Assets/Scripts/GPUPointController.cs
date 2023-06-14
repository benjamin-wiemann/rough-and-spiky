using UnityEngine;

using Unity.Mathematics;

public abstract class GPUPointController
{


    protected float[] debugSpectrogram;

    protected Material material;

    protected ComputeShader computeShader;

    protected enum ReadFromBuffer
    {
        A,
        B
    }

    protected ReadFromBuffer readFromBuffer = ReadFromBuffer.A;

    protected float cumulatedDeltaTime = 0;

    protected static readonly int
    resolutionId = Shader.PropertyToID("_Resolution"),
    indexOffsetId = Shader.PropertyToID("_IndexOffset"),
    depthId = Shader.PropertyToID("_Depth"),
    meshXId = Shader.PropertyToID("_MeshX"),
    meshZId = Shader.PropertyToID("_MeshZ"),
    heightId = Shader.PropertyToID("_HeightScale"),
    spectrumDeltaTimeId = Shader.PropertyToID("_SpectrumDeltaTime"),
    meshResolutionId = Shader.PropertyToID("_MeshResolution"),
    spectrogramId = Shader.PropertyToID("_Spectrogram"),
    prevSpectrogramId = Shader.PropertyToID("_PrevSpectrogram");


    public GPUPointController( Material material, ComputeShader computeShader, int nFreqBands, int depth )
    {
        this.material = material;
        this.computeShader = computeShader;
    }

    ~GPUPointController()
    {
        ReleaseBuffers();
    }

    public abstract void ReleaseBuffers();
    
    public void UpdatePointPosition(
        int nFreqBands,
        int depth, 
        float spectrumShiftTime, 
        float heightScale,
        int meshResolution, 
        float meshX,
        float meshZ,
        float[] spectrum,
        Mesh mesh,
        bool debug)
    {
        float spectrumDeltaTime;
        if( !debug )
        {
            int kernelHandle = computeShader.FindKernel("SpectrumVisualizer");

            cumulatedDeltaTime += Time.deltaTime;
            int indexOffset = Mathf.FloorToInt(cumulatedDeltaTime / spectrumShiftTime);

            computeShader.SetInt(resolutionId, nFreqBands);
            computeShader.SetInt(indexOffsetId, indexOffset);
            computeShader.SetInt(depthId, depth);
            SendSpectrumToShader(computeShader, depth, spectrumShiftTime, spectrum, kernelHandle, indexOffset);

            int groupsX = Mathf.CeilToInt(nFreqBands / 8f);
            int groupsY = Mathf.CeilToInt(depth / 8f);
            computeShader.Dispatch(kernelHandle, groupsX, groupsY, 1);
            BindToMaterial();     
            spectrumDeltaTime = (cumulatedDeltaTime % spectrumShiftTime) / (spectrumShiftTime * (depth -1));
                   
        }
        else
        {
            SetDebugSpectrogram(nFreqBands, depth);
            spectrumDeltaTime = 0;
        }
        material.SetInteger(resolutionId, nFreqBands);
        material.SetInteger(depthId, depth);
        material.SetFloat(heightId, heightScale);
        material.SetFloat(meshXId, meshX);
        material.SetFloat(meshZId, meshZ);
        material.SetFloat(meshResolutionId, (float) meshResolution);
        material.SetFloat(spectrumDeltaTimeId, spectrumDeltaTime);
    }

    protected abstract void SetDebugSpectrogram(int resolution, int depth);


    protected abstract void SendSpectrumToShader(
        ComputeShader computeShader, 
        int depth, 
        float spectrumShiftTime, 
        float[] spectrum, 
        int kernelHandle, 
        int indexOffset);
    

    protected abstract void BindToMaterial();

}