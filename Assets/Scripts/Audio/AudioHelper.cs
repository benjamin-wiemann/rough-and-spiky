using UnityEngine;

namespace Audio
{
    
    public class AudioHelper
    {

        public enum InterpolationType{
            Linear,
            Logarithmic
        }

        // Computes frequency bands of a spectrum.
        // The array indices marks the samples of spectrum at which one frequency band ends and the next one begins

        public void ComputeFrequencyBands(ref float[] bands, 
            float[] spectrum, 
            float[] indices, 
            InterpolationType interpolation, 
            bool shiftRight,
            bool reverse)
        {
            
            float indexInterval = 0;
            float interpolatedAmplitude = 0;
            float delta = 1;

            for( int i = 0; i < indices.Length - 1 ; i++ )
            {
                // Start new index interval
                float average = interpolatedAmplitude * ( 1- delta);    
                indexInterval = 1 - delta;        
                
                for( int j = (int) Mathf.Ceil(indices[i]); j < indices[i+1] && indices[i+1] - j >= 1; j++)
                {
                    average += spectrum[j];
                    indexInterval++;
                }
                // Interpolate value at _indices[i+1]
                if( Mathf.Ceil( indices[i+1]) != indices[i+1] )
                {
                    int lowerIndex = (int) Mathf.Floor( indices[i+1] );
                    int upperIndex = (int) Mathf.Ceil( indices[i+1] );
                    delta = indices[i+1] - lowerIndex;
                    interpolatedAmplitude = Mathf.Lerp( spectrum[lowerIndex], spectrum[upperIndex],  delta); 
                    average += interpolatedAmplitude * delta;
                    indexInterval += delta;
                }
                            
                average /= indexInterval;
                if (bands.Length == 2 * (indices.Length - 1 ) && shiftRight)
                {
                    if ( reverse )
                    {
                        bands[(indices.Length - 1) * 2 - i - 1] = average;
                    }
                    else
                    {
                        bands[i + indices.Length - 1] = average;
                    }                    
                }
                else
                {
                    if ( reverse )
                    {
                        bands[indices.Length - 2 - i] = average;
                    }
                    else
                    {
                        bands[i] = average;
                    }                    
                }
            }
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

        public void ConvertToDb( ref float[] dbSignal, float[] signal)
        {            
            for ( int i=0; i < signal.Length; i++)
            {
                dbSignal[i] = 20 * Mathf.Log10( signal[i] );
            }
        }

        public void InterleaveChannelBuffers(ref float[] dest, float[] inputA, float[] inputB)
        {
            // Put the data samples in alternating order
            for (int i = 0; i < dest.Length; i++)
            {
                if (i % 2 == 0)
                {
                    dest[i] = inputA[i / 2];
                }
                else
                {
                    dest[i] = inputB[(i -1) / 2];
                }
            }
        }

    }

}