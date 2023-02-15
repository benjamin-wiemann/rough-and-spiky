using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ShaderTest : MonoBehaviour
{
        
    [SerializeField]
    ComputeShader computeShader;

    RenderTexture texture1;
    RenderTexture texture2;

    int kernelHandle;

    static readonly int renderTexture1Id = Shader.PropertyToID("_RenderTexture1");
    static readonly int renderTexture2Id = Shader.PropertyToID("_RenderTexture2");

    // Start is called before the first frame update
    void Start()
    {
        texture1 = new RenderTexture(128, 128, 0);
        texture1.enableRandomWrite = true;
        texture1.Create();
        kernelHandle = computeShader.FindKernel("TestShaderMain");
        GetComponent<MeshRenderer>().material.SetTexture("_TestTexture", texture1);        
    }

    // Update is called once per frame
    void Update()
    {
        computeShader.SetTexture(kernelHandle, renderTexture1Id, texture1);
        computeShader.SetTexture(kernelHandle, renderTexture2Id, texture1);
        computeShader.Dispatch(kernelHandle, 9, 9, 1);
    }

}
