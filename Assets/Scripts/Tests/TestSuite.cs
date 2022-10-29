using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

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

    }

    [Test]
    public void ComputeFrequencyBandAmplitudesWithFloatIndices()
    {
        AudioHelper helper = new AudioHelper();
       
        float[] linearSpectrum= new float[]{
            1f,1f,1f,1f
        };     
        float[] testIndices = new float[]{ 0f, 0.59f, 1.52f, 3f};
        float[] bands = helper.ComputeFrequencyBands( linearSpectrum, testIndices, AudioHelper.InterpolationType.Linear );
        Assert.That( bands, Is.EqualTo( new float[3]{1f, 1f, 1f} ) );

    }

}
