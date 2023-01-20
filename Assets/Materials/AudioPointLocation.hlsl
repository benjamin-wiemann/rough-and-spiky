
StructuredBuffer<float> _Positions;
int _Resolution;
int _Depth;
float _HeightScale;

void SpectrumPosition_float ( float3 positionIn, out float3 positionOut) {
	int index = (int) round(positionIn.x * _Resolution)  + (int) round(positionIn.z * _Resolution) * _Depth;
	positionOut = float3( positionIn.x, _Positions[index] * _HeightScale, positionIn.z);
}

// void SpectrumPosition_half ( half3 positionIn, out half3 positionOut) {
// 	int index = (int) round(positionIn.x * _Resolution)  + (int) round(positionIn.z * _Resolution) * _Resolution;
// 	positionOut = (half3) _Positions[index];
// }

// void ShaderGraphFunction_float (float3 In, out float3 Out) {
// 	Out = In;
// }

// void ShaderGraphFunction_half (half3 In, out half3 Out) {
// 	Out = In;
// }