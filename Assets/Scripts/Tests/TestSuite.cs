using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;

using static CPUPointControlHelper;

public class TestSuite
{

    [Test]
    public void ComputeFrequencyBandIndices()
    {
        AudioHelper helper = new AudioHelper();
        int rawSpectrumLength = 4;
        int numBands = 3;        
        float[] testIndices = new float[]{ 0f, 0.59f, 1.52f, 3f};
        float[] indices = helper.ComputeFrequencyBandIndices( rawSpectrumLength, numBands );
        Assert.That( indices, Is.EqualTo( testIndices ).Within(0.01));       
        
    }

    [Test]
    public void ComputeFrequencyBandAmplitudesWithIntegerIndices()
    {
        AudioHelper helper = new AudioHelper();
        
        float[] linearSpectrum= new float[]{
             1f,1f,1f,1f,1f,1f,1f,1f
        }; 
        float[] integerIndices = new float[]{ 0f, 1f, 3f, 7f};
        float[] bands = helper.ComputeFrequencyBands( linearSpectrum, integerIndices, AudioHelper.InterpolationType.Linear );
        Assert.That( bands, Is.EqualTo( new float[3]{1f, 1f, 1f} ) );
        linearSpectrum = new float[]{
            0,0,0,1f,1f,1f,1f,1f
        };
        bands = helper.ComputeFrequencyBands( linearSpectrum, integerIndices, AudioHelper.InterpolationType.Linear );
        Assert.That( bands, Is.EqualTo( new float[3]{0, 0, 1} ) );

    }

    [Test]
    public void ComputeFrequencyBandAmplitudesWithFloatIndices()
    {
        AudioHelper helper = new AudioHelper();
       
        float[] linearSpectrum= new float[]{
            0,1f,1f,1f
        };     
        float[] testIndices = new float[]{ 0f, 0.5f, 1.5f, 3f};
        float[] bands = helper.ComputeFrequencyBands( linearSpectrum, testIndices, AudioHelper.InterpolationType.Linear );
        Assert.That( bands, Is.EqualTo( new float[3]{0.5f, (0.5f * 0.5f) + (1 * 0.5f) , 1f} ) );

    }

    [Test]
    public void SetStartPositionsJob()
    {
        JobHandle jobHandle = default;
        NativeArray<float3> positions = new NativeArray<float3>(16, Allocator.Persistent);
        int resolution = 4;
        jobHandle = new SetStartPositionsJob {
            positions = positions,
            resolution = resolution
        }.Schedule( positions.Length, jobHandle );
        jobHandle.Complete();
        Assert.That( positions[0].x, Is.EqualTo( -0.75f ));
        Assert.That( positions[0].z, Is.EqualTo( -0.75f ));
        Assert.That( positions[1].x, Is.EqualTo( -0.25f ));
        Assert.That( positions[4].x, Is.EqualTo( -0.75f ));
        Assert.That( positions[4].z, Is.EqualTo( -0.25f ));
        positions.Dispose();
    }

    [Test]
    public void UpdatePointPositionsJob()
    {
        JobHandle jobHandle = default;
        float3[] initArray = new Unity.Mathematics.float3[16];
        for ( int i=0; i < initArray.Length; i++)
        {
            initArray[i] = new Unity.Mathematics.float3(0,0,0);
        }
        NativeArray<float3> positionsA = new NativeArray<float3>(initArray, Allocator.Persistent);
        NativeArray<float3> positionsB = new NativeArray<float3>(initArray, Allocator.Persistent);
        float[] freqBands = {1f, 1f, 1f, 1f};
        NativeArray<float> frequencyBands = new NativeArray<float>( freqBands, Allocator.Persistent);
        int resolution = 4;
        int depth = 4;
        float heightScale = 1f;
        
        int indexOffset = 0;
        jobHandle = new UpdatePointPositionsJob {
            frequencyBands = frequencyBands,
            positions = positionsB,
            prevPositions = positionsA,
            resolution = resolution,
            depth = depth,
            heightScale = heightScale,
            indexOffset = indexOffset
        }.Schedule( positionsB.Length, jobHandle );
        jobHandle.Complete();
        Assert.That( positionsB[15].y, Is.EqualTo( 0f ));
        Assert.That( positionsB[11].y, Is.EqualTo( 0f ));

        indexOffset = 1;
        jobHandle = new UpdatePointPositionsJob {
            frequencyBands = frequencyBands,
            positions = positionsB,
            prevPositions = positionsA,
            resolution = resolution,
            depth = depth,
            heightScale = heightScale,
            indexOffset = indexOffset
        }.Schedule( positionsB.Length, jobHandle );
        jobHandle.Complete();
        Assert.That( positionsB[15].y, Is.EqualTo( 1f ));
        Assert.That( positionsB[11].y, Is.EqualTo( 0f ));

        NativeArray<float3> positionsC = new NativeArray<float3>(initArray, Allocator.Persistent);
        indexOffset = 1;
        jobHandle = new UpdatePointPositionsJob {
            frequencyBands = frequencyBands,
            positions = positionsC,
            prevPositions = positionsB,
            resolution = resolution,
            depth = depth,
            heightScale = heightScale,
            indexOffset = indexOffset
        }.Schedule( positionsC.Length, jobHandle );
        jobHandle.Complete();
        Assert.That( positionsC[15].y, Is.EqualTo( 1f ));
        Assert.That( positionsC[11].y, Is.EqualTo( 1f ));
        Assert.That( positionsC[7].y, Is.EqualTo( 0f ));
        
        positionsA.Dispose();
        positionsB.Dispose();
        positionsC.Dispose();
        frequencyBands.Dispose();
    }

}
