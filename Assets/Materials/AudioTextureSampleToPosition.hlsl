int _Resolution;
int _Depth;
float _MeshX;
float _MeshZ;

void SpectrumPosition_float ( float2 UVIn, out float3 NormalOut, out float3 TangentOut) {
	// clamp overlapping triangles when calculating x index
	// and map uv coordinates to the index interval
	float u = clamp(UVIn.x, 0, 1) * (_Resolution) - 0.5/(_Resolution); 
	float v = clamp(UVIn.y, 0, 1) * (_Depth) - 0.5/(_Depth); 
	
	float deltaU = u - clamp(floor(u), 0, _Resolution - 1);
	float deltaV = v - clamp(floor(v), 0, _Depth - 1);

	// get all four closest samples in the spectrogram
	float lowUlowV = _SpectrogramTexture[uint2(clamp(floor(u), 0, _Resolution - 1), clamp(floor(v), 0, _Depth - 1))];
	float lowUhighV = _SpectrogramTexture[uint2(clamp(floor(u), 0, _Resolution - 1), clamp(ceil(v), 0, _Depth - 1))];
	float highUlowV = _SpectrogramTexture[uint2(clamp(ceil(u), 0, _Resolution - 1), clamp(floor(v), 0, _Depth - 1))];
	float highUhighV = _SpectrogramTexture[uint2(clamp(ceil(u), 0, _Resolution - 1), clamp(ceil(v), 0, _Depth - 1))];

	float lowV = (lerp(lowUlowV, highUlowV, deltaU) + _Offset) / _Offset;
	float highV = (lerp(lowUhighV, highUhighV, deltaU) + _Offset) / _Offset;
	float lowU = (lerp(lowUlowV, lowUhighV, deltaV) + _Offset) / _Offset;
	float highU = (lerp(highUlowV, highUhighV, deltaV) + _Offset) / _Offset;	

	float2 derivatives = float2( (highU - lowU) * _HeightScale * _Resolution / _MeshX,  (highV - lowV) * _HeightScale * _Depth / _MeshZ);
	TangentOut = float3(1.0, derivatives.x , 0.0);
	NormalOut = cross(float3(0.0, derivatives.y, 1.0), TangentOut);
}
