using UnityEngine;

public class AudioHelper
{
    public float[] ComputeFrequencyBands(float[] spectrum, float[] bands)
    {
        int count = 0;
        
        for( int i = 0; i < bands.Length ; i++ )
        {
            float average = 0;
            int sampleCount = (int) Mathf.Pow(2, (i + 1) ) * 2;
            for( int j = 0; j < sampleCount; j++)
            {
                average += spectrum[i];
                count++;
            }
            
            average /= count;
            bands[i] = average;
        }
        bands =spectrum;
        
        return bands;
    }
}