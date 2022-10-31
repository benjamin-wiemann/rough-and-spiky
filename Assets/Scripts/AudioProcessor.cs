using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class AudioProcessor : MonoBehaviour
{
    
    [SerializeField]
    private Visualizer visualizer;

    [SerializeField]
    private AudioHelper.InterpolationType interpolation = AudioHelper.InterpolationType.Linear;

	private AudioSource _audioSource;

    private float[] _spectrum = new float[512];

    private float[] _freqBandIndices;
    public float[] _freqBands;

    AudioHelper helper = new AudioHelper();


    // int resolution

    void OnEnable()
    {
        _audioSource = GetComponent<AudioSource> ();
        Initialize( visualizer.xResolution );
        
        Visualizer.resChangeEvent += Initialize;        
    }

    void OnDisable()
    {
        Visualizer.resChangeEvent -= Initialize;
    }

    void Initialize( int resolution)
    {
        _freqBands = new float[resolution];
        _freqBandIndices = helper.ComputeFrequencyBandIndices(_spectrum.Length, resolution);
    }

    public float[] GetSpectrumAudioSource()
	{
		_audioSource.GetSpectrumData( _spectrum, 0, FFTWindow.BlackmanHarris );        
        return helper.ComputeFrequencyBands( _spectrum, _freqBandIndices, interpolation );
	}

}
