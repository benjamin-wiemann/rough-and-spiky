StructuredBuffer<float> _Spectrogram;
int _Resolution;
int _Depth;
float _HeightScale;
float _MeshX;
float _MeshZ;
float _SpectrumDeltaTime;
float _TriangleHeight;
float _TriangleWidth;

typedef struct 
{
  float lowU;
  float highU;
  float lowV;
  float highV;
  float middle;
} interpolationset;


float SampleBuffer(in float2 uvIndex) 
{
	float uIndex = clamp(uvIndex.x, 0, _Resolution -1);
	float vIndex = clamp(uvIndex.y, 0, _Depth - 1);
	float2 delta = uvIndex - floor(uvIndex);
	float lowXLowZ = _Spectrogram[floor(uIndex) + floor(vIndex) * _Resolution];
	float lowXHighZ = _Spectrogram[floor(uIndex) + ceil(vIndex) * _Resolution];
	float highXLowZ = _Spectrogram[ceil(uIndex) + floor(vIndex) * _Resolution];
	float highXHighZ = _Spectrogram[ceil(uIndex) + ceil(vIndex) * _Resolution];

	float2 interpolated;
	interpolated.x = (lerp(lowXLowZ, lowXHighZ, delta.y));
	interpolated.y = (lerp(highXLowZ, highXHighZ, delta.y));

	return (lerp(interpolated.x, interpolated.y, delta.x) + _Offset) / _Offset;
}

// float ApplyOffset(float val)
// {
// 	return val + _Offset / _Offset;
// }

float3 GetDerivativeU( interpolationset set)
{
	return float3( normalize(float2(_TriangleWidth, (set.highU - set.middle) * _HeightScale)) 
			+ normalize(float2( _TriangleWidth, (set.middle - set.lowU) * _HeightScale)), 
			0);	
}

float3 GetDerivativeV( interpolationset set)
{
	return float3( 0,
	 		normalize(float2((set.highV - set.middle) * _HeightScale, _TriangleHeight)) 
			+ normalize(float2((set.middle - set.lowV) * _HeightScale, _TriangleHeight)) );
}

void SpectrumPosition_float ( in float3 PositionIn, in float2 UVIn, out float3 PositionOut, out float3 NormalOut, out float3 TangentOut) 
{
	float2 uvIndex;
	uvIndex.x = UVIn.x * (_Resolution - 1); 
	uvIndex.y =  UVIn.y * (_Depth - 1) + _SpectrumDeltaTime;

	float2 uInterval = float2(_TriangleWidth / _MeshX, 0);
	float2 vInterval = float2(0, _TriangleHeight / _MeshZ);
	// float2 uInterval = float2(1 / _Resolution, 0);
	// float2 vInterval = float2(0, 2 / (sqrt(3) * _Resolution) );

	interpolationset set;
	set.lowU = SampleBuffer(uvIndex - uInterval);
	set.highU = SampleBuffer(uvIndex + uInterval);
	set.lowV = SampleBuffer(uvIndex - vInterval);
	set.highV = SampleBuffer(uvIndex + vInterval);
	set.middle = SampleBuffer(uvIndex);
	
	// float2 derivatives = float2( (highX - lowX) * _HeightScale * _Resolution / _MeshX,  (highZ - lowZ) * _HeightScale * _Depth / _MeshZ);
	TangentOut = GetDerivativeU( set );
	float3 derivativeV = GetDerivativeV( set );
	NormalOut = cross(derivativeV, TangentOut);
	PositionOut = float3( PositionIn.x, set.middle * _HeightScale, PositionIn.z);
}




