using UnityEngine;

internal class SpectrumTextureController : GPUPointController
{
    
    RenderTexture textureA, textureB;

    ComputeBuffer spectrumBuffer;

    static readonly int spectrumId = Shader.PropertyToID("_Spectrum");

    public SpectrumTextureController(Material material, int maxResolution, int depth) : base(material, maxResolution, depth)
    {
        textureA = new RenderTexture(maxResolution, depth, 0);
        textureA.enableRandomWrite = true;
        textureA.Create();
        textureB = new RenderTexture(maxResolution, depth, 0);
        textureB.enableRandomWrite = true;
        textureB.Create(); 
        spectrumBuffer = new ComputeBuffer(maxResolution, 4);       
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
        if (readFromBuffer == ReadFromBuffer.A)
        {
            computeShader.SetTexture(kernelHandle, prevSpectrogramId, textureA);
            computeShader.SetTexture(kernelHandle, spectrogramId, textureB);
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