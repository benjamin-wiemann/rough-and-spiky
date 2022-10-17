using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class AudioProcessor : MonoBehaviour
{
    
	private AudioSource _audioSource;

	public static float[] _samples = new float[512];
    public static float[] _freqBand = new float[8];

    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource> ();
        float prevBand = 0f;
        for( int i = 0; i < _freqBand.Length; i++)
        {
            _freqBand[i] = prevBand + 22050 / Mathf.Pow(2, _freqBand.Length - i); 
            prevBand = _freqBand[i];
        }
    }

    void GetSpectrumAudioSource()
	{
		_audioSource.GetSpectrumData(_samples, 0, FFTWindow.Blackman);
        for( int i = 0; i < _samples.Length ; i++ )
        {

        }
	}

    void GetSpectrumAtTime(float u, float v, float t)
    {

    }
}
