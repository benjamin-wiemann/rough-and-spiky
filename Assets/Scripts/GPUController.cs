using UnityEngine;

public class GPUController
{

    ComputeBuffer freqBandsBuffer;

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
    positionsId = Shader.PropertyToID("_Positions"),
    prevPositionsId = Shader.PropertyToID("_PrevPositions"),
    frequencyBandsId = Shader.PropertyToID("_FrequencyBands"),
    resolutionId = Shader.PropertyToID("_Resolution"),
    stepId = Shader.PropertyToID("_Step"),
    indexOffsetId = Shader.PropertyToID("_IndexOffset"),
    depthId = Shader.PropertyToID("_Depth"),
    heightId = Shader.PropertyToID("_HeightScale");

    public GPUController(int maxResolution)
    {
        positionsBufferA = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
        positionsBufferB = new ComputeBuffer(maxResolution * maxResolution, 3 * 4);
        freqBandsBuffer = new ComputeBuffer(maxResolution, 4);
    }

    ~GPUController()
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
        if (freqBandsBuffer != null)
        {
            freqBandsBuffer.Release();
            freqBandsBuffer = null;
        }
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

        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetInt(indexOffsetId, Mathf.FloorToInt(cumulatedDeltaTime));
        computeShader.SetInt(depthId, depth);
        computeShader.SetFloat(heightId, heightScale);

        freqBandsBuffer.SetData(spectrum);

        if (readFromBuffer == ReadFromBuffer.A)
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

        int groupsX = Mathf.CeilToInt(resolution / 8f);
        int groupsY = Mathf.CeilToInt(depth / 8f);
        computeShader.Dispatch(kernelHandle, groupsX, groupsY, 1);

        if (readFromBuffer == ReadFromBuffer.A)
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

        if (cumulatedDeltaTime - Mathf.FloorToInt(speed * Time.deltaTime) >= 1)
        {
            cumulatedDeltaTime -= Mathf.FloorToInt(cumulatedDeltaTime);
        }
    }
}