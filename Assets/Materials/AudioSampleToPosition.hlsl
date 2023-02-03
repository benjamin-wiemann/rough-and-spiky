StructuredBuffer<float> _Spectrogram;
int _Resolution;
int _Depth;
float _HeightScale;
float _MeshX;
float _MeshZ;

void SpectrumPosition_float ( float3 PositionIn, out float3 PositionOut, out float3 NormalOut, out float3 TangentOut) {
	// clamp overlapping triangles when calculating x index
	float x = ( clamp(PositionIn.x, -_MeshX/2, _MeshX/2) + _MeshX/2) * (_Resolution - 1) / _MeshX; 
	float deltaX = x - floor(x);
	float z =  PositionIn.z * _Depth / _MeshZ;
	float deltaZ = z - floor(z);

	// get all four closest samples in the spectrogram
	float lowXLowZ = _Spectrogram[floor(x) + floor(z) * _Resolution];
	float lowXHighZ = _Spectrogram[floor(x) + ceil(z) * _Resolution];
	float highXLowZ = _Spectrogram[ceil(x) + floor(z) * _Resolution];
	float highXHighZ = _Spectrogram[ceil(x) + ceil(z) * _Resolution];

	float lowZ = lerp(lowXLowZ, highXLowZ, deltaX);
	float highZ = lerp(lowXHighZ, highXHighZ, deltaX);
	// interpolate on z axis
	PositionOut = float3( PositionIn.x, lerp(lowZ, highZ, deltaZ) * _HeightScale, PositionIn.z);

	float lowX = lerp(highXLowZ, highXHighZ, deltaX);
	float highX = lerp(lowXLowZ, lowXHighZ, deltaX);

	float2 derivatives = float2( (highX - lowX) * _Resolution,  (highZ - lowZ) * _Resolution);
	TangentOut = float3(1.0, derivatives.x, 0.0);
	NormalOut = cross(float3(0.0, derivatives.y, 1.0), TangentOut);
}
