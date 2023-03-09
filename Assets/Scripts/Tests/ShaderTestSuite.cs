using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections;

public class ShaderTestSuite
{

    protected static readonly int
    spectrogramId = Shader.PropertyToID("_Spectrogram"),
    prevSpectrogramId = Shader.PropertyToID("_PrevSpectrogram"),    
    indexOffsetId = Shader.PropertyToID("_IndexOffset"),    
    meshXId = Shader.PropertyToID("_MeshX"),
    meshZId = Shader.PropertyToID("_MeshZ"),
    heightId = Shader.PropertyToID("_HeightScale"),
    spectrumDeltaTimeId = Shader.PropertyToID("_SpectrumDeltaTime"),
    triangleHeightId = Shader.PropertyToID("_TriangleHeight"),
    triangleWidthId = Shader.PropertyToID("_TriangleWidth"),
    resolutionId = Shader.PropertyToID("_Resolution"),
    depthId = Shader.PropertyToID("_Depth"),
    offsetId = Shader.PropertyToID("_Offset");

    ComputeShader GetShader()
    {
        GameObject testObject = GameObject.Find("ShaderTestObject");
        Assert.IsNotNull(testObject);
        ShaderTestDummy dummy = testObject.GetComponent<ShaderTestDummy>();
        Assert.IsNotNull(dummy);
        return dummy.computeShader;
    }

    [UnityTest]
    public IEnumerator SampleBufferInterpolatesCorrectly()
    {
        SceneManager.LoadScene("TestingScene", LoadSceneMode.Additive);
        yield return null;
        Scene scene = SceneManager.GetSceneByName("TestingScene");
        SceneManager.SetActiveScene(scene);
            
        ComputeShader computeShader = GetShader();

        int resultId = Shader.PropertyToID("_Result"),
        uvIndexId = Shader.PropertyToID("_UvIndex");  
        
        ComputeBuffer resultBuffer = new ComputeBuffer(1, 4);
        ComputeBuffer spectrogramBuffer = new ComputeBuffer(4, 4);
        float[] spectrogram = {-150f, -100f, -50f, 0f};
        spectrogramBuffer.SetData(spectrogram);
        int kernel = computeShader.FindKernel("TestSampleBuffer");
        computeShader.SetBuffer(kernel, spectrogramId, spectrogramBuffer);
        computeShader.SetBuffer(kernel, resultId, resultBuffer);
        
        computeShader.SetInt(depthId, 2);        
        computeShader.SetInt(resolutionId, 2);
        computeShader.SetFloat(offsetId, 150f);
        float[] uvIndex = {0.5f, 0.5f, 0, 0};
        computeShader.SetFloats( uvIndexId, uvIndex );
        computeShader.Dispatch(kernel, 1, 1, 1);
        float[] result = new float[1];
        resultBuffer.GetData(result);
        Assert.That( result[0], Is.EqualTo(0.5f).Within(0.000001f) );
        
        spectrogramBuffer.Release();
        resultBuffer.Release();
        
    }

    [UnityTest]
    public IEnumerator GetDerivativeReturnsAngleBisector()
    {
        SceneManager.LoadScene("TestingScene", LoadSceneMode.Additive);
        yield return null;
        Scene scene = SceneManager.GetSceneByName("TestingScene");
        SceneManager.SetActiveScene(scene);

        int setId = Shader.PropertyToID("_Set");

        ComputeShader computeShader = GetShader();
        computeShader.SetFloat(triangleHeightId, 1f);
        computeShader.SetFloat(triangleWidthId, 1f);
        computeShader.SetFloat(heightId, 1f);
        float[] interpolationSet = {1f, 0, 0, 0, 3f, 0, 0, 0,  1f, 0, 0, 0, 3f, 0, 0, 0, 2.5f, 0,0,0};
        computeShader.SetFloats( setId, interpolationSet );

        int resultId = Shader.PropertyToID("_Result");
        int kernel = computeShader.FindKernel("TestGetDerivativeU");
        ComputeBuffer resultBuffer = new ComputeBuffer(3, 4);
        computeShader.SetBuffer(kernel, resultId, resultBuffer);
        computeShader.Dispatch(kernel, 1, 1, 1);
        float[] result = new float[3];
        resultBuffer.GetData(result);
        Assert.That( result[2], Is.EqualTo(0) );
        Assert.That( Mathf.Atan(result[1] / result[0]), Is.EqualTo((Mathf.Atan(0.5f) + Mathf.Atan(1.5f)) / 2 ).Within(0.000001f) );

        kernel = computeShader.FindKernel("TestGetDerivativeV");
        computeShader.SetBuffer(kernel, resultId, resultBuffer);
        computeShader.Dispatch(kernel, 1, 1, 1);
        resultBuffer.GetData(result);
        Assert.That( result[0], Is.EqualTo(0) );
        Assert.That( Mathf.Atan(result[1] / result[2]), Is.EqualTo((Mathf.Atan(0.5f) + Mathf.Atan(1.5f)) / 2 ).Within(0.000001f) );

        resultBuffer.Release();
        
    }
}
