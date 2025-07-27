# Rough and Spiky
This app uses audio spectrum data to create a spectrogram, which is used by a shader to show an visual representation of the data. It is based on the Universal Render Pipeline.  
### How it works
The app uses the [JACK Audio Connection Kit](https://jackaudio.org/) to receive an audio signal from various audio sources running on the system. For each frame, it receives the current audio spectrum of both channels of a stereo signal and sends it to a compute shader. The latter attaches the spectrum to a spectrogram. This spectrogram is finally sampled by a vertex shader, which transforms a simple plane to a tunnel shape. It then uses the spectrogram data to modify the radius of each vertex of the mesh. The right and the left halfs correspond to the left and right channels of the stereo signal.  
The result is a psychedelic looking, hypnotic tunnel which moves accordingly to the audio signal. To get an impression, watch the video below. It's a screencast of the app, recorded together with appropriate psychedelic music ;\).  

[![](https://img.youtube.com/vi/4CR7CPT6Fdo/0.jpg)](https://youtu.be/4CR7CPT6Fdo "Youtube Video")

