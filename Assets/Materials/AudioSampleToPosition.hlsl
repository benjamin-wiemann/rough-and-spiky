StructuredBuffer<float> _Spectrogram;
int _Depth;
float _HeightScale;
// float _MeshX;
// float _MeshZ;
// float _SpectrumDeltaTime;
float _TriangleHeight;
float _TriangleWidth;
int _Resolution;

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

// This function interpolates between the buffer indices which are closest to uvIndex
// It returns the result a well as the gradient in U and V direction
t_BSample SampleBuffer(in float2 uvIndex) 
{
	t_BSample result;
	float uIndex = clamp(abs(uvIndex.x), 0, (float) _Resolution -1);
	float vIndex = clamp(abs(uvIndex.y), 0, (float) _Depth - 1);
	float2 delta = abs(uvIndex) - floor(abs(uvIndex));

	float lowULowV = _Spectrogram[floor(uIndex) + floor(clamp(vIndex - 1, 0, _Depth -1)) * (float) _Resolution];
	float lowUMidV = _Spectrogram[floor(uIndex) + floor(vIndex) * (float) _Resolution];
	float lowUHighV = _Spectrogram[floor(uIndex) + ceil(vIndex) * (float) _Resolution];

	float highULowV = _Spectrogram[ceil(uIndex) + floor(clamp(vIndex - 1, 0, (float) _Depth -1)) * (float) _Resolution];
	float highUMidV = _Spectrogram[ceil(uIndex) + floor(vIndex) * (float) _Resolution];
	float highUHighV = _Spectrogram[ceil(uIndex) + ceil(vIndex) * (float) _Resolution];

	// Apply smoothing depending on if _Smooth flag is set
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

void SpectrumPosition_float ( in float3 PositionIn, in float2 UVIn, out float3 PositionOut) 
{
	float2 uvIndex;
	uvIndex.x = UVIn.x * (_Resolution - 1); 
	uvIndex.y =  UVIn.y * (_Depth - 1) + sign(UVIn.y) * ( _AccelerateStart * pow(_SpectrumDeltaTime, 2) + (1 - _AccelerateStart) * _SpectrumDeltaTime );

	t_BSample spectrumSample = SampleBuffer(uvIndex);
	
	PositionOut = float3( PositionIn.x, spectrumSample.value * _HeightScale, PositionIn.z );
}
