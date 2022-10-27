using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class AudioProcessor : MonoBehaviour
{
    
    [SerializeField]
    private Visualizer visualizer;

	private AudioSource _audioSource;

    float[] _spectrum = new float[512];
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
        // float prevBand = 0f;
        // for( int i = 0; i < _freqBand.Length; i++)
        // {
        //     _freqBand[i] = prevBand + 22050 / Mathf.Pow(2, _freqBand.Length - i); 
        //     prevBand = _freqBand[i];
        // }
    }

    public float[] GetSpectrumAudioSource()
	{
		_audioSource.GetSpectrumData(_spectrum, 0, FFTWindow.Blackman);
        return helper.ComputeFrequencyBands( _spectrum, _freqBands);
	}

}
