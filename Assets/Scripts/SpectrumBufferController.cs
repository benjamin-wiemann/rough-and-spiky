using UnityEngine;

internal class SpectrumBufferController : GPUPointController
{

    ComputeBuffer positionsBufferA;

    ComputeBuffer positionsBufferB;

    public SpectrumBufferController(Material material, ComputeShader computeShader, int nFreqBands, int depth) : 
        base(material, computeShader, nFreqBands, depth)
    {
        positionsBufferA = new ComputeBuffer(nFreqBands * ( depth), 4);
        positionsBufferB = new ComputeBuffer(nFreqBands * ( depth), 4);
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
                positionsBufferA.SetData(spectrum, 0, spectrum.Length * (depth - 1), spectrum.Length);
                cumulatedDeltaTime -= indexOffset * spectrumShiftTime;
            }
            computeShader.SetBuffer(kernelHandle, prevSpectrogramId, positionsBufferA);
            computeShader.SetBuffer(kernelHandle, spectrogramId, positionsBufferB);
        }
        else
        {
            if (indexOffset > 0)
            {
                positionsBufferB.SetData(spectrum, 0, spectrum.Length * (depth - 1), spectrum.Length);
                cumulatedDeltaTime -= indexOffset * spectrumShiftTime;
            }
            computeShader.SetBuffer(kernelHandle, prevSpectrogramId, positionsBufferB);
            computeShader.SetBuffer(kernelHandle, spectrogramId, positionsBufferA);
        }
    }

    protected override void SetDebugSpectrogram(int resolution, int depth)
    {
        int debugSpectrumKernel = computeShader.FindKernel("SetDebugSpectrogram");
        computeShader.SetBuffer(debugSpectrumKernel, spectrogramId, positionsBufferA);
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetInt(depthId, depth);
        int groupsX = Mathf.CeilToInt(resolution / 8f);
        int groupsY = Mathf.CeilToInt(depth / 8f);
        computeShader.Dispatch(debugSpectrumKernel, groupsX, groupsY, 1);
        material.SetBuffer(spectrogramId, positionsBufferA);
    }
    
}