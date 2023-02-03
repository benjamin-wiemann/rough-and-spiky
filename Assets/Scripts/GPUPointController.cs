using UnityEngine;

using Unity.Mathematics;

public class GPUPointController
{

    ComputeBuffer positionsBufferA;

    ComputeBuffer positionsBufferB;

    float[] debugSpectrogram;

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
    heightId = Shader.PropertyToID("_HeightScale"),
    meshXId = Shader.PropertyToID("_MeshX"),
    meshZId = Shader.PropertyToID("_MeshZ");

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

    public void UpdatePointPosition(
        ComputeShader computeShader,
        int resolution,
        int depth, 
        int speed, 
        float heightScale, 
        float meshX,
        float meshZ,
        float[] spectrum, 
        Material material, 
        Mesh mesh,
        bool debugShader)
    {
        if( !debugShader )
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
            material.SetFloat(meshXId, meshX);
            material.SetFloat(meshZId, meshZ);

            if (cumulatedDeltaTime - Mathf.FloorToInt(speed * Time.deltaTime) >= 1)
            {
                cumulatedDeltaTime -= Mathf.FloorToInt(cumulatedDeltaTime);
            }
        }
        else
        {            
            GenerateDebugSpectrogram( resolution, depth);  
            
            positionsBufferA.SetData(debugSpectrogram);
            material.SetBuffer(spectrogramId, positionsBufferA);
            material.SetInteger(resolutionId, resolution);
            material.SetInteger(depthId, depth);
            material.SetFloat(heightId, heightScale);
            material.SetFloat(meshXId, meshX);
            material.SetFloat(meshZId, meshZ);

        }
    }

    void GenerateDebugSpectrogram(int resolution, int depth)
    {
        bool init = false;
        if( debugSpectrogram == null)
        {
            debugSpectrogram = new float[depth * resolution];
            init = true;
        }
        if( debugSpectrogram.Length != depth * resolution)
        {
            debugSpectrogram = new float[depth * resolution];
            init = true;
        }
        if( init )
        {
            for( int i = 0; i < depth; i++ )
            {
                for( int j = 0; j < resolution; j++ )
                {
                    debugSpectrogram[j + i * resolution] = 100 *(Mathf.Sin(2 * (float) i / (float) depth * Mathf.PI) + Mathf.Sin(2 * (float) j / (float) resolution * Mathf.PI));
                }
            }
        }
    }
}