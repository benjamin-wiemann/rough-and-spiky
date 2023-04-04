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
    spectrogramId = Shader.PropertyToID("_Spectrogram"),
    prevSpectrogramId = Shader.PropertyToID("_PrevSpectrogram"),
    resolutionId = Shader.PropertyToID("_Resolution"),
    indexOffsetId = Shader.PropertyToID("_IndexOffset"),
    depthId = Shader.PropertyToID("_Depth"),
    meshXId = Shader.PropertyToID("_MeshX"),
    meshZId = Shader.PropertyToID("_MeshZ"),
    heightId = Shader.PropertyToID("_HeightScale"),
    spectrumDeltaTimeId = Shader.PropertyToID("_SpectrumDeltaTime"),
    triangleHeightId = Shader.PropertyToID("_TriangleHeight"),
    triangleWidthId = Shader.PropertyToID("_TriangleWidth"),
    meshResolutionId = Shader.PropertyToID("_MeshResolution");

    public GPUPointController( Material material, ComputeShader computeShader, int maxResolution, int depth )
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
        int spectrumResolution,
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

            computeShader.SetInt(resolutionId, spectrumResolution);
            computeShader.SetInt(indexOffsetId, indexOffset);
            computeShader.SetInt(depthId, depth);
            SendSpectrumToShader(computeShader, depth, spectrumShiftTime, spectrum, kernelHandle, indexOffset);

            int groupsX = Mathf.CeilToInt(spectrumResolution / 8f);
            int groupsY = Mathf.CeilToInt(depth / 8f);
            computeShader.Dispatch(kernelHandle, groupsX, groupsY, 1);
            BindToMaterial();     
            spectrumDeltaTime = (cumulatedDeltaTime % spectrumShiftTime) / spectrumShiftTime;
                   
        }
        else
        {
            SetDebugSpectrogram(spectrumResolution, depth);
            spectrumDeltaTime = 0;
        }
        material.SetInteger(resolutionId, spectrumResolution);
        material.SetInteger(depthId, depth);
        material.SetFloat(heightId, heightScale);
        material.SetFloat(meshXId, meshX);
        material.SetFloat(meshZId, meshZ);
        material.SetFloat(meshResolutionId, (float) meshResolution);
        float triangleHeight = meshZ / Mathf.Round( meshResolution * meshZ * 2f / Mathf.Sqrt(3f) );
        float triangleWidth = meshX / Mathf.Round( meshResolution * meshX);
        material.SetFloat(triangleHeightId, triangleHeight);
        material.SetFloat(triangleWidthId, triangleWidth);
        material.SetFloat(spectrumDeltaTimeId, spectrumDeltaTime);
        // Debug.Log("Triangle width: " + triangleWidth + " height: " + triangleHeight);        
    }

    protected abstract void SetDebugSpectrogram(int resolution, int depth);


    protected abstract void SendSpectrumToShader(ComputeShader computeShader, int depth, float spectrumShiftTime, float[] spectrum, int kernelHandle, int indexOffset);
    

    protected abstract void BindToMaterial();

}