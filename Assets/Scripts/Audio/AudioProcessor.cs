using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Audio
{

    public enum Channel
    {
        Left = 0,
        Right = 1,
        Stereo = 2,
        StereoMirrorAtLow = 3,
        StereoMirrorAtHigh = 4
    }

    [RequireComponent(typeof(AudioSource))]
    public class AudioProcessor : MonoBehaviour
    {

        [SerializeField]
        private Visualizer visualizer;

        [SerializeField]
        private AudioHelper.InterpolationType interpolationType = AudioHelper.InterpolationType.Linear;

        readonly int spectrumLength = 512;

        private AudioSource _audioSource;

        float[] spectrum;

        float[] freqBandIndices;
        float[] freqBands;
        float[] freqBandsStereo;

        AudioHelper helper = new AudioHelper();

        void OnEnable()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        public void Initialize(int nFreqBandsPerSpectrum, Channel channel)
        {
            spectrum = new float[spectrumLength];
            if(channel == Channel.Left || channel == Channel.Right)
            {
                freqBands = new float[nFreqBandsPerSpectrum];
            }
            else
            {
                freqBands = new float[nFreqBandsPerSpectrum * 2];
            }
            
            freqBandIndices = helper.ComputeFrequencyBandIndices(spectrumLength, nFreqBandsPerSpectrum);
        }

        public void GetSpectrumAudioSource(ref float[] dbSpectrum, Channel channel)
        {         
            
            switch (channel)
            {
                case Channel.Left:
                    _audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
                    helper.ComputeFrequencyBands(ref freqBands, spectrum, freqBandIndices, interpolationType, false, false);
                    break;
                case Channel.Right:
                    _audioSource.GetSpectrumData(spectrum, 1, FFTWindow.BlackmanHarris);
                    helper.ComputeFrequencyBands(ref freqBands, spectrum, freqBandIndices, interpolationType, false, false);
                    break;
                case Channel.Stereo:
                    _audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
                    helper.ComputeFrequencyBands(ref freqBands, spectrum, freqBandIndices, interpolationType, false, false);
                    _audioSource.GetSpectrumData(spectrum, 1, FFTWindow.BlackmanHarris);
                    helper.ComputeFrequencyBands(ref freqBands, spectrum, freqBandIndices, interpolationType, true, false);           
                    break;
                case Channel.StereoMirrorAtLow:
                    _audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
                    helper.ComputeFrequencyBands(ref freqBands, spectrum, freqBandIndices, interpolationType, false, true);
                    _audioSource.GetSpectrumData(spectrum, 1, FFTWindow.BlackmanHarris);
                    helper.ComputeFrequencyBands(ref freqBands, spectrum, freqBandIndices, interpolationType, true, false);
                    break;
                case Channel.StereoMirrorAtHigh:
                    _audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
                    helper.ComputeFrequencyBands(ref freqBands, spectrum, freqBandIndices, interpolationType, false, false);
                    _audioSource.GetSpectrumData(spectrum, 1, FFTWindow.BlackmanHarris);
                    helper.ComputeFrequencyBands(ref freqBands, spectrum, freqBandIndices, interpolationType, true, true);
                    break;

            }
            helper.ConvertToDb(ref dbSpectrum, freqBands);
        }

    }

}