using UnityEngine;

internal class SpectrumTextureController : GPUPointController
{
    
    RenderTexture textureA, textureB;

    ComputeBuffer spectrumBuffer;

    static readonly int 
    spectrumId = Shader.PropertyToID("_Spectrum"),
    spectrogramTextureId = Shader.PropertyToID("_SpectrogramTexture"),
    debugBufferId = Shader.PropertyToID("_DebugBuffer");

    public SpectrumTextureController(Material material, ComputeShader computeShader, int resolution, int depth) : base(material, computeShader, resolution, depth)
    {

        textureA = new RenderTexture(resolution, depth, 0, RenderTextureFormat.RFloat);
        textureA.enableRandomWrite = true;
        textureA.Create();
        textureB = new RenderTexture(resolution, depth, 0, RenderTextureFormat.RFloat);
        textureB.enableRandomWrite = true;
        textureB.Create(); 
        spectrumBuffer = new ComputeBuffer(resolution, 4); 
    }

    public override void ReleaseBuffers()
    {
        if(textureA != null)
        {
            textureA.Release();
            textureA = null;
        }
        if(textureB != null)
        {
            textureB.Release();
            textureB = null;
        }        
        if(spectrumBuffer != null)
        {
            spectrumBuffer.Release();
            spectrumBuffer = null;
        }
    }

    protected override void SetDebugSpectrogram( int resolution, int depth )
    {
        int debugSpectrumKernel = computeShader.FindKernel("SetDebugSpectrogram");
        computeShader.SetTexture(debugSpectrumKernel, spectrogramId, textureA);
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetInt(depthId, depth);
        int groupsX = Mathf.CeilToInt(resolution / 8f);
        int groupsY = Mathf.CeilToInt(depth / 8f);
        computeShader.Dispatch(debugSpectrumKernel, groupsX, groupsY, 1);
        material.SetTexture(spectrogramTextureId, textureA);
    }

    protected override void SendSpectrumToShader(ComputeShader computeShader, int depth, float spectrumShiftTime, float[] spectrum, int kernelHandle, int indexOffset)
    {
        spectrumBuffer.SetData(spectrum);            
        cumulatedDeltaTime -= indexOffset * spectrumShiftTime;
        int spectrumKernel = computeShader.FindKernel("SetSpectrum");
        computeShader.SetBuffer(spectrumKernel, spectrumId, spectrumBuffer);
        if (readFromBuffer == ReadFromBuffer.A)
        {
            computeShader.SetTexture(spectrumKernel, prevSpectrogramId, textureA);
            computeShader.SetTexture(kernelHandle, prevSpectrogramId, textureA);
            computeShader.SetTexture(kernelHandle, spectrogramId, textureB);
        }
        else
        {
            computeShader.SetTexture(spectrumKernel, prevSpectrogramId, textureB);
            computeShader.SetTexture(kernelHandle, prevSpectrogramId, textureB);
            computeShader.SetTexture(kernelHandle, spectrogramId, textureA);
        }
        int groupsX = Mathf.CeilToInt(spectrum.Length / 8f);
        computeShader.Dispatch(spectrumKernel, groupsX, 1, 1);
    }

    protected override void BindToMaterial()
    {
        if (readFromBuffer == ReadFromBuffer.A)
        {
            material.SetTexture(spectrogramTextureId, textureB);
            readFromBuffer = ReadFromBuffer.B;
        }
        else
        {
            material.SetTexture(spectrogramTextureId, textureA);
            readFromBuffer = ReadFromBuffer.A;
        }        
    }
}