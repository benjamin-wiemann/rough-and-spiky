using NUnit.Framework;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public class TestSuite
{

    [Test]
    public void ComputeFrequencyBandIndices()
    {
        Audio.Helper helper = new Audio.Helper();
        int rawSpectrumLength = 4;
        int numBands = 3;
        float[] testIndices = new float[] { 0f, 0.59f, 1.52f, 3f };
        float[] indices = helper.ComputeFrequencyBandIndices(rawSpectrumLength, numBands);
        Assert.That(indices, Is.EqualTo(testIndices).Within(0.01));

    }

    [Test]
    public void ComputeFrequencyBandAmplitudesWithIntegerIndices()
    {
        Audio.Helper helper = new Audio.Helper();

        float[] linearSpectrum = new float[]{
             1f,1f,1f,1f,1f,1f,1f,1f
        };
        float[] integerIndices = new float[] { 0f, 1f, 3f, 7f };
        float[] bands = helper.ComputeFrequencyBands(linearSpectrum, integerIndices, Audio.Helper.InterpolationType.Linear);
        Assert.That(bands, Is.EqualTo(new float[3] { 1f, 1f, 1f }));
        linearSpectrum = new float[]{
            0,0,0,1f,1f,1f,1f,1f
        };
        bands = helper.ComputeFrequencyBands(linearSpectrum, integerIndices, Audio.Helper.InterpolationType.Linear);
        Assert.That(bands, Is.EqualTo(new float[3] { 0, 0, 1 }));

    }

    [Test]
    public void ComputeFrequencyBandAmplitudesWithFloatIndices()
    {
        Audio.Helper helper = new Audio.Helper();

        float[] linearSpectrum = new float[]{
            0,1f,1f,1f
        };
        float[] testIndices = new float[] { 0f, 0.5f, 1.5f, 3f };
        float[] bands = helper.ComputeFrequencyBands(linearSpectrum, testIndices, Audio.Helper.InterpolationType.Linear);
        Assert.That(bands, Is.EqualTo(new float[3] { 0.5f, (0.5f * 0.5f) + (1 * 0.5f), 1f }));

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
