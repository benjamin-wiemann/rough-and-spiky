using NUnit.Framework;
using UnityEngine;
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
        float[] testIndices = new float[] { 0f, 0.59f, 1.52f, 3f };
        float[] indices = helper.ComputeFrequencyBandIndices(rawSpectrumLength, numBands);
        Assert.That(indices, Is.EqualTo(testIndices).Within(0.01));

    }

    [Test]
    public void ComputeFrequencyBandAmplitudesWithIntegerIndices()
    {
        AudioHelper helper = new AudioHelper();

        float[] linearSpectrum = new float[]{
             1f,1f,1f,1f,1f,1f,1f,1f
        };
        float[] integerIndices = new float[] { 0f, 1f, 3f, 7f };
        float[] bands = helper.ComputeFrequencyBands(linearSpectrum, integerIndices, AudioHelper.InterpolationType.Linear);
        Assert.That(bands, Is.EqualTo(new float[3] { 1f, 1f, 1f }));
        linearSpectrum = new float[]{
            0,0,0,1f,1f,1f,1f,1f
        };
        bands = helper.ComputeFrequencyBands(linearSpectrum, integerIndices, AudioHelper.InterpolationType.Linear);
        Assert.That(bands, Is.EqualTo(new float[3] { 0, 0, 1 }));

    }

    [Test]
    public void ComputeFrequencyBandAmplitudesWithFloatIndices()
    {
        AudioHelper helper = new AudioHelper();

        float[] linearSpectrum = new float[]{
            0,1f,1f,1f
        };
        float[] testIndices = new float[] { 0f, 0.5f, 1.5f, 3f };
        float[] bands = helper.ComputeFrequencyBands(linearSpectrum, testIndices, AudioHelper.InterpolationType.Linear);
        Assert.That(bands, Is.EqualTo(new float[3] { 0.5f, (0.5f * 0.5f) + (1 * 0.5f), 1f }));

    }

    [Test]
    public void SetStartPositionsJob()
    {
        JobHandle jobHandle = default;
        NativeArray<float3> positions = new NativeArray<float3>(16, Allocator.Persistent);
        int resolution = 4;
        jobHandle = new SetStartPositionsJob
        {
            positions = positions,
            resolution = resolution
        }.Schedule(positions.Length, jobHandle);
        jobHandle.Complete();
        Assert.That(positions[0].x, Is.EqualTo(-0.75f));
        Assert.That(positions[0].z, Is.EqualTo(-0.75f));
        Assert.That(positions[1].x, Is.EqualTo(-0.25f));
        Assert.That(positions[4].x, Is.EqualTo(-0.75f));
        Assert.That(positions[4].z, Is.EqualTo(-0.25f));
        positions.Dispose();
    }

    [Test]
    public void UpdatePointPositionsJob()
    {
        JobHandle jobHandle = default;
        float3[] initArray = new Unity.Mathematics.float3[16];
        for (int i = 0; i < initArray.Length; i++)
        {
            initArray[i] = new Unity.Mathematics.float3(0, 0, 0);
        }
        NativeArray<float3> positionsA = new NativeArray<float3>(initArray, Allocator.Persistent);
        NativeArray<float3> positionsB = new NativeArray<float3>(initArray, Allocator.Persistent);
        float[] freqBands = { 1f, 1f, 1f, 1f };
        NativeArray<float> frequencyBands = new NativeArray<float>(freqBands, Allocator.Persistent);
        int resolution = 4;
        int depth = 4;
        float heightScale = 1f;

        int indexOffset = 0;
        jobHandle = new UpdatePointPositionsJob
        {
            frequencyBands = frequencyBands,
            positions = positionsB,
            prevPositions = positionsA,
            resolution = resolution,
            depth = depth,
            heightScale = heightScale,
            indexOffset = indexOffset
        }.Schedule(positionsB.Length, jobHandle);
        jobHandle.Complete();
        Assert.That(positionsB[15].y, Is.EqualTo(0f));
        Assert.That(positionsB[11].y, Is.EqualTo(0f));

        indexOffset = 1;
        jobHandle = new UpdatePointPositionsJob
        {
            frequencyBands = frequencyBands,
            positions = positionsB,
            prevPositions = positionsA,
            resolution = resolution,
            depth = depth,
            heightScale = heightScale,
            indexOffset = indexOffset
        }.Schedule(positionsB.Length, jobHandle);
        jobHandle.Complete();
        Assert.That(positionsB[15].y, Is.EqualTo(1f));
        Assert.That(positionsB[11].y, Is.EqualTo(0f));

        NativeArray<float3> positionsC = new NativeArray<float3>(initArray, Allocator.Persistent);
        indexOffset = 1;
        jobHandle = new UpdatePointPositionsJob
        {
            frequencyBands = frequencyBands,
            positions = positionsC,
            prevPositions = positionsB,
            resolution = resolution,
            depth = depth,
            heightScale = heightScale,
            indexOffset = indexOffset
        }.Schedule(positionsC.Length, jobHandle);
        jobHandle.Complete();
        Assert.That(positionsC[15].y, Is.EqualTo(1f));
        Assert.That(positionsC[11].y, Is.EqualTo(1f));
        Assert.That(positionsC[7].y, Is.EqualTo(0f));

        positionsA.Dispose();
        positionsB.Dispose();
        positionsC.Dispose();
        frequencyBands.Dispose();
    }

    [Test]
    public void TriangleGrid()
    {
        float testTolerance = 0.00001f;

        Mesh mesh = new Mesh
        {
            name = "Procedural Mesh"
        };
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];
        ProceduralMesh.SharedTriangleGrid triangleGrid = new ProceduralMesh.SharedTriangleGrid();
        triangleGrid.Resolution = 1;
        triangleGrid.dimZ = 1;
        triangleGrid.dimX = 1;
        ProceduralMesh.SingleStream stream = new ProceduralMesh.SingleStream();
        stream.Setup(
            meshData,
            mesh.bounds = triangleGrid.Bounds,
            triangleGrid.VertexCount,
            triangleGrid.IndexCount
        );
        
        for (int i = 0; i < triangleGrid.JobLength; i++)
        {
            triangleGrid.Execute<ProceduralMesh.SingleStream>(i, stream);
        }

        Assert.That(meshData.vertexCount, Is.EqualTo(4));
        NativeArray<Vector3> outVertices = new NativeArray<Vector3>(4, Allocator.Persistent);
        meshData.GetVertices(outVertices);
        Assert.That(outVertices[0].x, Is.EqualTo(-0.75f).Within(testTolerance));
        Assert.That(outVertices[0].z, Is.EqualTo(0f).Within(testTolerance));
        Assert.That(outVertices[3].x, Is.EqualTo(0.75f).Within(testTolerance));
        Assert.That(outVertices[3].z, Is.EqualTo(1f).Within(testTolerance));
        NativeArray<Vector2> uv = new NativeArray<Vector2>(4, Allocator.Persistent);
        meshData.GetUVs(0,uv);
        Assert.That(uv[0].x, Is.EqualTo(0f).Within(testTolerance));
        Assert.That(uv[0].y, Is.EqualTo(0f).Within(testTolerance));
        Assert.That(uv[3].x, Is.EqualTo(1f).Within(testTolerance));
        Assert.That(uv[3].y, Is.EqualTo(1).Within(testTolerance));
        outVertices.Dispose();
        uv.Dispose();

        triangleGrid.Resolution = 2;
        triangleGrid.dimZ = 2;
        triangleGrid.dimX = 2;
        stream.Setup(
            meshData,
            mesh.bounds = triangleGrid.Bounds,
            triangleGrid.VertexCount,
            triangleGrid.IndexCount
        );
        for (int i = 0; i < triangleGrid.JobLength; i++)
        {
            triangleGrid.Execute<ProceduralMesh.SingleStream>(i, stream);
        }   
        Assert.That( meshData.vertexCount, Is.EqualTo( 30 ));
        outVertices = new NativeArray<Vector3>(30, Allocator.Persistent);
        meshData.GetVertices( outVertices );     
        Assert.That(outVertices[0].x, Is.EqualTo(-1.125f).Within(testTolerance));
        Assert.That(outVertices[0].z, Is.EqualTo(0f).Within(testTolerance));
        Assert.That(outVertices[29].x, Is.EqualTo(1.125f).Within(testTolerance));
        Assert.That(outVertices[29].z, Is.EqualTo(2 ).Within(testTolerance));   
        uv = new NativeArray<Vector2>(30, Allocator.Persistent);
        meshData.GetUVs(0,uv);
        Assert.That(uv[0].x, Is.EqualTo(0f).Within(testTolerance));
        Assert.That(uv[0].y, Is.EqualTo(0f).Within(testTolerance));
        Assert.That(uv[24].x, Is.EqualTo(4f/4.5f).Within(testTolerance));
        Assert.That(uv[29].x, Is.EqualTo(1f).Within(testTolerance));
        Assert.That(uv[29].y, Is.EqualTo(1f).Within(testTolerance));
        outVertices.Dispose();
        uv.Dispose();
               

        triangleGrid.Resolution = 1;
        triangleGrid.dimZ = 1;
        triangleGrid.dimX = 1.2f;
        stream.Setup(
            meshData,
            mesh.bounds = triangleGrid.Bounds,
            triangleGrid.VertexCount,
            triangleGrid.IndexCount
        );
        for (int i = 0; i < triangleGrid.JobLength; i++)
        {
            triangleGrid.Execute<ProceduralMesh.SingleStream>(i, stream);
        }   
        Assert.That( meshData.vertexCount, Is.EqualTo( 4 ));
        outVertices = new NativeArray<Vector3>(4, Allocator.Persistent);
        meshData.GetVertices( outVertices );     
        Assert.That(outVertices[0].x, Is.EqualTo(-0.9f).Within(testTolerance));
        Assert.That(outVertices[0].z, Is.EqualTo(0f).Within(testTolerance));
        Assert.That(outVertices[3].x, Is.EqualTo(0.9f).Within(testTolerance));
        Assert.That(outVertices[3].z, Is.EqualTo(1).Within(testTolerance));
        outVertices.Dispose();
        meshDataArray.Dispose();

    }

}
