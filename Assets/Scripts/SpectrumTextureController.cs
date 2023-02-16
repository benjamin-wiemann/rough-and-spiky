using UnityEngine;

internal class SpectrumTextureController : GPUPointController
{
    
    RenderTexture textureA, textureB;

    ComputeBuffer spectrumBuffer;

    static readonly int spectrumId = Shader.PropertyToID("_Spectrum");

    public SpectrumTextureController(Material material, int resolution, int depth) : base(material, resolution, depth)
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

    protected override void SetDebugSpectrogram()
    {
        throw new System.NotImplementedException();
    }

    protected override void SendSpectrumToShader(ComputeShader computeShader, int depth, float spectrumShiftTime, float[] spectrum, int kernelHandle, int indexOffset)
    {
        if (indexOffset > 0)
        {

            spectrumBuffer.SetData(spectrum);            
            cumulatedDeltaTime -= indexOffset * spectrumShiftTime;
        }
        computeShader.SetBuffer(kernelHandle, spectrumId, spectrumBuffer);
        if (readFromBuffer == ReadFromBuffer.A)
        {
            computeShader.SetTexture(0, prevSpectrogramId, textureA);
            computeShader.SetTexture(0, spectrogramId, textureB);
        }
        else
        {
            computeShader.SetTexture(kernelHandle, prevSpectrogramId, textureB);
            computeShader.SetTexture(kernelHandle, spectrogramId, textureA);
        }
    }

    protected override void BindToMaterial()
    {
        if (readFromBuffer == ReadFromBuffer.A)
        {
            material.SetTexture(spectrogramId, textureB);
            readFromBuffer = ReadFromBuffer.B;
        }
        else
        {
            material.SetTexture(spectrogramId, textureA);
            readFromBuffer = ReadFromBuffer.A;
        }        
    }
}