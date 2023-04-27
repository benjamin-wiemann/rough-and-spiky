using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Audio{

    [RequireComponent (typeof (AudioSource))]
    public class AudioProcessor : MonoBehaviour
    {
        
        [SerializeField]
        private Visualizer visualizer;

        [SerializeField]
        private Helper.InterpolationType interpolation = Helper.InterpolationType.Linear;

        private AudioSource _audioSource;

        private float[] _spectrum = new float[512];

        private float[] _freqBandIndices;
        public float[] _freqBands;

        Helper helper = new Helper();


        // int resolution

        void OnEnable()
        {
            _audioSource = GetComponent<AudioSource> ();
            Initialize( visualizer.spectrumResolution );           
        }

        void OnDisable()
        {
            
        }

        public void Initialize( int resolution)
        {
            _freqBands = new float[resolution];
            _freqBandIndices = helper.ComputeFrequencyBandIndices(_spectrum.Length, resolution);
        }

        public float[] GetSpectrumAudioSource()
        {
            _audioSource.GetSpectrumData( _spectrum, 0, FFTWindow.BlackmanHarris );        
            _freqBands = helper.ComputeFrequencyBands( _spectrum, _freqBandIndices, interpolation );
            return helper.ConvertToDb( _freqBands );
        }

    }

}