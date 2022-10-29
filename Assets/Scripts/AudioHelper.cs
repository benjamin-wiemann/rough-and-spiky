using UnityEngine;

public class AudioHelper
{

    public enum InterpolationType{
        Linear,
        Logarithmic
    }

    // Computes frequency bands of a spectrum.
    // The array indices marks the samples of spectrum at which one frequency band ends and the next one begins
    public float[] ComputeFrequencyBands(float[] spectrum, float[] indices, InterpolationType interpolation)
    {
               
        float[] bands = new float[indices.Length - 1];   

        // The exponent step factor defines the size of a step on a logarithmic scale
        
        float indexInterval = 0;
        float interpolatedAmplitude = 0;
        float delta = 1;

        for( int i = 0; i < indices.Length - 1 ; i++ )
        {
            // Start new index interval
            float average = interpolatedAmplitude * ( 1- delta);    
            indexInterval = 1 - delta;        
            
            for( int j = (int) Mathf.Ceil(indices[i]); j < indices[i+1]; j++)
            {
                average += spectrum[j];
                indexInterval++;
            }
            // Interpolate amplitude at _indices[i]
            if( i < indices.Length - 1 && Mathf.Ceil( indices[i]) != indices[i] )
            {
                int lowerIndex = (int) Mathf.Floor( indices[i] );
                int upperIndex = (int) Mathf.Ceil( indices[i] );
                delta = indices[i] - lowerIndex;
                interpolatedAmplitude = Interpolate( spectrum[lowerIndex], spectrum[upperIndex],  delta, interpolation);
                average += interpolatedAmplitude * delta;
                indexInterval += delta;
            }
                        
            average /= indexInterval;
            bands[i] = average;
        }
                
        return bands;
    }

    public float[] ComputeFrequencyBandIndices( int spectrumLength, int numBands)
    {
        float[] indices = new float[numBands+1];
        float expStepFactor = Mathf.Log(spectrumLength, 2) / (indices.Length - 1);
        for( int i = 0; i < indices.Length ; i++ )
        {
             indices[i] = Mathf.Pow(2, expStepFactor * i ) - 1;
        }
        return indices;
    }

    private float Interpolate(float left, float right, float delta, InterpolationType interpolation)
    {
        if( interpolation == InterpolationType.Linear)
        {
            return left * delta + right * (1-delta);
        }
        return 0f;
    }

}