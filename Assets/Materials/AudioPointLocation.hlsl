
StructuredBuffer<float> _Spectrogram;
int _Resolution;
int _Depth;
float _HeightScale;

void SpectrumPosition_float ( float3 positionIn, out float3 positionOut) {
	int index = (int) round(positionIn.x * _Resolution)  + (int) round(positionIn.z * _Resolution) * _Depth;
	positionOut = float3( positionIn.x, _Spectrogram[index] * _HeightScale, positionIn.z);
}
