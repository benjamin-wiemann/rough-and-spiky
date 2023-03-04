StructuredBuffer<float> _Spectrogram;
int _Resolution;
int _Depth;
float _HeightScale;
float _MeshX;
float _MeshZ;
float _SpectrumDeltaTime;
float _TriangleHeight;
float _TriangleWidth;


float SampleBuffer(in float2 uvIndex) 
{
	float uIndex = clamp(uvIndex.x, 0, _Resolution -1);
	float vIndex = clamp(uvIndex.y, 0, _Depth - 1);
	float2 delta = uvIndex - floor(uvIndex);
	float lowXLowZ = _Spectrogram[floor(uIndex) + floor(vIndex) * _Resolution];
	float lowXHighZ = _Spectrogram[floor(uIndex) + ceil(vIndex) * _Resolution];
	float highXLowZ = _Spectrogram[ceil(uIndex) + floor(vIndex) * _Resolution];
	float highXHighZ = _Spectrogram[ceil(uIndex) + ceil(vIndex) * _Resolution];

	float4 uvOut;
	uvOut.w = (lerp(lowXLowZ, highXLowZ, delta.x) + _Offset) / _Offset;
	uvOut.x = (lerp(lowXHighZ, highXHighZ, delta.x) + _Offset) / _Offset;
	uvOut.y = (lerp(lowXLowZ, lowXHighZ, delta.y) + _Offset) / _Offset;
	uvOut.z = (lerp(highXLowZ, highXHighZ, delta.y) + _Offset) / _Offset;

	return lerp(uvOut.y, uvOut.z, delta.x);
}

void SpectrumPosition_float ( in float3 PositionIn, in float2 UVIn, out float3 PositionOut, out float3 NormalOut, out float3 TangentOut) 
{
	
	float2 uvIndex;
	uvIndex.x = UVIn.x * (_Resolution - 1); 
	uvIndex.y =  UVIn.y * (_Depth - 1) + _SpectrumDeltaTime;

	PositionOut = float3(PositionIn.x, SampleBuffer(uvIndex) * _HeightScale, PositionIn.z);
	
	float2 derivativesU;
	float2 uInterval = float2(_TriangleWidth / _MeshX, 0);
	float2x3 neighborsU = float2x3( 
		PositionIn.x - _TriangleWidth, SampleBuffer(uvIndex - uInterval) * _HeightScale, PositionIn.z,
		PositionIn.x + _TriangleWidth, SampleBuffer(uvIndex + uInterval) * _HeightScale, PositionIn.z);
	
	float2 derivativesV;
	float2 vInterval = float2(0, _TriangleHeight / _MeshZ);
	float2x3 neighborsV = float2x3( 
		PositionIn.x, SampleBuffer(uvIndex - vInterval) * _HeightScale, PositionIn.z - _TriangleWidth,
		PositionIn.x, SampleBuffer(uvIndex + vInterval) * _HeightScale, PositionIn.z + _TriangleWidth);
	
	// TangentOut = TangentOut/ abs(TangentOut);
	// NormalOut = normalize(cross(float3(0.0, derivatives.y, 1.0), float3(1.0, derivatives.x , 0.0)));
	
	TangentOut = normalize(neighborsU._21_22_23 - PositionOut) + normalize(PositionOut - neighborsU._11_12_13);
	float3 derivativeV = normalize(neighborsV._21_22_23 - PositionOut) + normalize(PositionOut - neighborsV._11_12_13);
	NormalOut = cross(derivativeV, TangentOut);

}


