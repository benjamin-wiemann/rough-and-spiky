
StructuredBuffer<float> _Spectrogram;
int _Resolution;
int _Depth;
float _HeightScale;
float _MeshX;
float _MeshZ;

void SpectrumPosition_float ( float3 positionIn, out float3 positionOut) {
	float x = clamp(positionIn.x, -_MeshX/2, _MeshX/2); // clamp overlapping triangles
	int index = (int) round( (x + _MeshX/2) * (_Resolution - 1) / _MeshX )  + (int) round( positionIn.z * _Depth / _MeshZ ) * _Resolution;
	positionOut = float3( positionIn.x, _Spectrogram[index] * _HeightScale, positionIn.z);
}
