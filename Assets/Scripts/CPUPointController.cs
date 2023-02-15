using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;
using static CPUPointControlHelper;


public class CPUPointController
{
    enum ArrayReadFrom{
        A,
        B
    }

    ArrayReadFrom arrayReadFrom;

    NativeArray<float3> positionsA;

    NativeArray<float3> positionsB;

    NativeArray<float> frequencyBands;

    ComputeBuffer positionsBuffer;

    float cumulatedDeltaTime = 0f;

    static readonly int
    positionsId = Shader.PropertyToID("_Positions"),
    stepId = Shader.PropertyToID("_Step");


    public CPUPointController( int resolution, int depth )
    {
        
        positionsA = new NativeArray<float3>( resolution * depth, Allocator.Persistent);
        positionsB = new NativeArray<float3>( resolution * depth, Allocator.Persistent);
        frequencyBands = new NativeArray<float>( resolution, Allocator.Persistent );
        positionsBuffer = new ComputeBuffer(positionsA.Length, 4 * 3);

        JobHandle jobHandle = default;
        jobHandle = new SetStartPositionsJob {
            positions = positionsA,
            resolution = resolution
        }.ScheduleParallel( positionsA.Length, 5, jobHandle );

        jobHandle = new SetStartPositionsJob {
            positions = positionsB,
            resolution = resolution
        }.ScheduleParallel( positionsB.Length, 5, jobHandle );
        jobHandle.Complete();
    }

    ~CPUPointController()
    {
        ReleaseBuffers();
    }


    public void ReleaseBuffers()
    {
        positionsA.Dispose();
        positionsB.Dispose();
        frequencyBands.Dispose();
        positionsBuffer.Release();
        positionsBuffer = null;        
    }

    public void UpdatePointPosition(
        int resolution,
        int depth, 
        float spectrumShiftTime, 
        float heightScale, 
        float[] spectrum, 
        Material material, 
        Mesh mesh)
    {
        float step = 2f / resolution;
        cumulatedDeltaTime += spectrumShiftTime * Time.deltaTime;
        frequencyBands = new NativeArray<float>(spectrum, Allocator.Persistent);

        JobHandle jobHandle = default;

        if( arrayReadFrom == ArrayReadFrom.A)
        {
            jobHandle = new UpdatePointPositionsJob {
                frequencyBands = frequencyBands,
                positions = positionsB,
                prevPositions = positionsA,
                resolution = resolution,
                depth = depth,
                heightScale = heightScale,
                indexOffset = Mathf.FloorToInt(cumulatedDeltaTime)
            }.ScheduleParallel( positionsB.Length, 5, jobHandle );
            jobHandle.Complete();
            positionsBuffer.SetData(positionsB);
            arrayReadFrom = ArrayReadFrom.B;
        }
        else
        {
            jobHandle = new UpdatePointPositionsJob {
                frequencyBands = frequencyBands,
                positions = positionsA,
                prevPositions = positionsB,
                resolution = resolution,
                depth = depth,                
                heightScale = heightScale,
                indexOffset = Mathf.FloorToInt(cumulatedDeltaTime)
            }.ScheduleParallel( positionsA.Length, 5, jobHandle );
            jobHandle.Complete();
            positionsBuffer.SetData(positionsA);
            arrayReadFrom = ArrayReadFrom.A;
        }
        
        material.SetBuffer(positionsId, positionsBuffer); 
        material.SetFloat(stepId, step);
        var bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));
        Graphics.DrawMeshInstancedProcedural(mesh, 0, material, bounds, resolution * depth);

        if (cumulatedDeltaTime - Mathf.FloorToInt(spectrumShiftTime * Time.deltaTime) >= 1)
        {
            cumulatedDeltaTime -= Mathf.FloorToInt(cumulatedDeltaTime);
        }
    }

}


