using UnityEngine;

using Unity.Mathematics;

public class GPUPointController
{

    ComputeBuffer positionsBufferA;

    ComputeBuffer positionsBufferB;

    enum ReadFromBuffer
    {
        A,
        B
    }

    ReadFromBuffer readFromBuffer = ReadFromBuffer.A;

    float cumulatedDeltaTime = 0;

    static readonly int
    spectrogramId = Shader.PropertyToID("_Spectrogram"),
    prevSpectrogramId = Shader.PropertyToID("_PrevSpectrogram"),
    resolutionId = Shader.PropertyToID("_Resolution"),
    stepId = Shader.PropertyToID("_Step"),
    indexOffsetId = Shader.PropertyToID("_IndexOffset"),
    depthId = Shader.PropertyToID("_Depth"),
    heightId = Shader.PropertyToID("_HeightScale");

    public GPUPointController( int maxResolution, int depth )
    {
        positionsBufferA = new ComputeBuffer(maxResolution * ( depth + 1), 4);
        positionsBufferB = new ComputeBuffer(maxResolution * ( depth + 1), 4);
    }

    ~GPUPointController()
    {
        ReleaseBuffers();
    }

    public void ReleaseBuffers()
    {
        if (positionsBufferA != null)
        {
            positionsBufferA.Release();
            positionsBufferA = null;
        }
        if (positionsBufferB != null)
        {
            positionsBufferB.Release();
            positionsBufferB = null;
        }
    }

    void TransferSpectrumToBuffer( float[] spectrum, ComputeBuffer buffer, int depth, float heightScale )
    {
        float step = 2f / spectrum.Length;
        float z = (depth + 0.5f) * step - 1.0f;
        float3[] spectrumPositions = new float3[ spectrum.Length];
        for( int i = 0; i< spectrum.Length; i++)
        {   
            spectrumPositions[i] = new float3((i + 0.5f) * step - 1.0f, heightScale * spectrum[i], z);            
        }
        // adding spectrum at the end of the buffer
        buffer.SetData(spectrumPositions, 0, spectrum.Length * depth, spectrum.Length);
    }


    public void UpdatePointPosition(
        ComputeShader computeShader,
        int resolution,
        int depth, 
        int speed, 
        float heightScale, 
        float[] spectrum, 
        Material material, 
        Mesh mesh)
    {

        int kernelHandle = computeShader.FindKernel("SpectrumVisualizer");

        float step = 2f / resolution;
        cumulatedDeltaTime += speed * Time.deltaTime;
        int indexOffset = Mathf.FloorToInt(cumulatedDeltaTime);

        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetInt(indexOffsetId, indexOffset);
        computeShader.SetInt(depthId, depth);

        if (readFromBuffer == ReadFromBuffer.A)
        {
            if(indexOffset > 0)
            {
                positionsBufferA.SetData(spectrum, 0, spectrum.Length * depth, spectrum.Length);
            }            
            computeShader.SetBuffer(kernelHandle, prevSpectrogramId, positionsBufferA);
            computeShader.SetBuffer(kernelHandle, spectrogramId, positionsBufferB);
        }
        else
        {   
            if(indexOffset > 0)
            {
                positionsBufferB.SetData(spectrum, 0, spectrum.Length * depth, spectrum.Length);
            } 
            computeShader.SetBuffer(kernelHandle, prevSpectrogramId, positionsBufferB);
            computeShader.SetBuffer(kernelHandle, spectrogramId, positionsBufferA);
        }

        int groupsX = Mathf.CeilToInt(resolution / 8f);
        int groupsY = Mathf.CeilToInt(depth / 8f);
        computeShader.Dispatch(kernelHandle, groupsX, groupsY, 1);

        if (readFromBuffer == ReadFromBuffer.A)
        {
            material.SetBuffer(spectrogramId, positionsBufferB);
            readFromBuffer = ReadFromBuffer.B;
        }
        else
        {
            material.SetBuffer(spectrogramId, positionsBufferA);
            readFromBuffer = ReadFromBuffer.A;
        }

        material.SetInteger(resolutionId, resolution);
        material.SetInteger(depthId, depth);
        material.SetFloat(heightId, heightScale);
        // var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        // // Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * depth);

        if (cumulatedDeltaTime - Mathf.FloorToInt(speed * Time.deltaTime) >= 1)
        {
            cumulatedDeltaTime -= Mathf.FloorToInt(cumulatedDeltaTime);
        }
    }
}