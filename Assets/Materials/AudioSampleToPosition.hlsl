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
} t_InterpolationSet;

typedef struct
{
	float value;
	float uDiff;
	float vDiff;
} t_BSample;


t_BSample SampleBuffer(in float2 uvIndex) 
{
	t_BSample result;
	float uIndex = clamp(uvIndex.x, 0, _Resolution -1);
	float vIndex = clamp(uvIndex.y, 0, _Depth - 1);
	float2 delta = uvIndex - floor(uvIndex);

	float lowULowV = _Spectrogram[floor(uIndex) + floor(clamp(vIndex - 1, 0, _Resolution -1)) * _Resolution];
	float lowUMidV = _Spectrogram[floor(uIndex) + floor(vIndex) * _Resolution];
	float lowUHighV = (_Spectrogram[floor(uIndex) + ceil(vIndex) * _Resolution] + lowUMidV) / 2;

	float highULowV = _Spectrogram[ceil(uIndex) + floor(clamp(vIndex - 1, 0, _Resolution -1)) * _Resolution];
	float highUMidV = _Spectrogram[ceil(uIndex) + floor(vIndex) * _Resolution];
	float highUHighV = _Spectrogram[ceil(uIndex) + ceil(vIndex) * _Resolution];

	// Apply smoothing depending on if _Smooth parameter is true
	lowUHighV = (lowUHighV + lowUMidV * _Smooth ) / ( 1 + _Smooth);
	lowUMidV = (lowUMidV + lowULowV * _Smooth ) / ( 1 + _Smooth);
	
	highUHighV = (highUHighV + highUMidV * _Smooth ) / ( 1 + _Smooth);
	highUMidV = (highUMidV + highULowV * _Smooth ) / ( 1 + _Smooth);

	float2 vInterpolated;
	vInterpolated.x = (lerp(lowUMidV, lowUHighV, delta.y));
	vInterpolated.y = (lerp(highUMidV, highUHighV, delta.y));
	
	float2 uInterpolated;
	uInterpolated.x = (lerp(lowUMidV, highUMidV, delta.x));
	uInterpolated.y = (lerp(lowUHighV, highUHighV, delta.x));

	result.uDiff = (vInterpolated.y - vInterpolated.x) / _Offset;
	result.vDiff = (uInterpolated.y - uInterpolated.x) / _Offset;

	result.value = (lerp(vInterpolated.x, vInterpolated.y, delta.x) + _Offset) / _Offset;

	return result;
}

// float ApplyOffset(float val)
// {
// 	return val + _Offset / _Offset;
// }

float3 GetDerivativeU( t_InterpolationSet set)
{
	return float3( normalize(float2(_TriangleWidth, (set.highU - set.middle) * _HeightScale)) 
			+ normalize(float2( _TriangleWidth, (set.middle - set.lowU) * _HeightScale)), 
			0);	
}

float3 GetDerivativeV( t_InterpolationSet set)
{
	return float3( 0,
	 		normalize(float2((set.highV - set.middle) * _HeightScale, _TriangleHeight)) 
			+ normalize(float2((set.middle - set.lowV) * _HeightScale, _TriangleHeight))) ;
}

void SpectrumPosition_float ( in float3 PositionIn, in float2 UVIn, out float3 PositionOut, out float3 NormalOut, out float3 TangentOut) 
{
	float2 uvIndex;
	uvIndex.x = UVIn.x * (_Resolution - 1); 
	uvIndex.y =  UVIn.y * (_Depth - 1) + _SpectrumDeltaTime;

	// float2 uInterval = float2(_TriangleWidth / _MeshX, 0);
	// float2 vInterval = float2(0, _TriangleHeight / _MeshZ);
	// float2 uInterval = float2(1 / _Resolution, 0);
	// float2 vInterval = float2(0, 2 / (sqrt(3) * _Resolution) );

	// t_InterpolationSet set;
	// set.lowU = SampleBuffer(uvIndex - uInterval);
	// set.highU = SampleBuffer(uvIndex + uInterval);
	// set.lowV = SampleBuffer(uvIndex - vInterval);
	// set.highV = SampleBuffer(uvIndex + vInterval);
	// set.middle = SampleBuffer(uvIndex);
	t_BSample spectrumSample = SampleBuffer(uvIndex);
	
	// float2 derivatives = float2( (highU - lowU) * _HeightScale * _Resolution / _MeshX,  (highV - lowV) * _HeightScale * _Depth / _MeshZ);

	// TangentOut = GetDerivativeU( set );
	// float3 derivativeV = GetDerivativeV( set );
	TangentOut = float3( _MeshX / _Resolution, spectrumSample.uDiff * _HeightScale, 0 );
	float3 derivativeV = float3( 0, spectrumSample.vDiff * _HeightScale, _MeshZ / _Depth );
	NormalOut = cross( derivativeV, TangentOut );
	PositionOut = float3( PositionIn.x, spectrumSample.value * _HeightScale, PositionIn.z );
}
