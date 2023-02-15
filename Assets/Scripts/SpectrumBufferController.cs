using UnityEngine;

internal class SpectrumBufferController : GPUPointController
{

    ComputeBuffer positionsBufferA;

    ComputeBuffer positionsBufferB;

    public SpectrumBufferController(Material material, int maxResolution, int depth) : base(material, maxResolution, depth)
    {
        positionsBufferA = new ComputeBuffer(maxResolution * ( depth + 1), 4);
        positionsBufferB = new ComputeBuffer(maxResolution * ( depth + 1), 4);
    }

    protected override void BindToMaterial()
    {
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
    }

    public override void ReleaseBuffers()
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

    protected override void SendSpectrumToShader(ComputeShader computeShader, int depth, float spectrumShiftTime, float[] spectrum, int kernelHandle, int indexOffset)
    {
        if (readFromBuffer == ReadFromBuffer.A)
        {
            if (indexOffset > 0)
            {
                positionsBufferA.SetData(spectrum, 0, spectrum.Length * depth, spectrum.Length);
                cumulatedDeltaTime -= indexOffset * spectrumShiftTime;
            }
            computeShader.SetBuffer(kernelHandle, prevSpectrogramId, positionsBufferA);
            computeShader.SetBuffer(kernelHandle, spectrogramId, positionsBufferB);
        }
        else
        {
            if (indexOffset > 0)
            {
                positionsBufferB.SetData(spectrum, 0, spectrum.Length * depth, spectrum.Length);
                cumulatedDeltaTime -= indexOffset * spectrumShiftTime;
            }
            computeShader.SetBuffer(kernelHandle, prevSpectrogramId, positionsBufferB);
            computeShader.SetBuffer(kernelHandle, spectrogramId, positionsBufferA);
        }
    }

    protected override void SetDebugSpectrogram()
    {
        positionsBufferA.SetData(debugSpectrogram);
        material.SetBuffer(spectrogramId, positionsBufferA);
    }
    
}