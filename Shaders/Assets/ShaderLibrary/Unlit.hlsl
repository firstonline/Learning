#ifndef MYRP_UNLIT_INCLUDED
#define MYRP_UNLIT_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl" // requires for CBUFFER_START and CBUFFER_END

// buffer the object to world matrix to only get it per draw
CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
CBUFFER_END

// buffer the view projection matrix to only ger once per frame
CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END

// to cope with GPU instancing using the same M matrix for all objects we define below code UNITY_MATRIX_M to use unity_ObjectToWorld
// and if the GPU instancing is enabled the below include code will modify UNITY_MATRIX_M to take M matrix from array of Ms
#define UNITY_MATRIX_M unity_ObjectToWorld 
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

// store all colors in array for gpu instancing
UNITY_INSTANCING_BUFFER_START(PerInstance)
    UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

struct VertexInput 
{
    float4 pos : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID // needed for GPU instancing to know which element from Ms array to take
};

struct VertexOutput
{
    float4 clipPos : SV_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID // needed for GPU instancing to know which color to take from array
};

VertexOutput UnlitPassVertex(VertexInput input)
{
    VertexOutput output;
    UNITY_SETUP_INSTANCE_ID(input); // set the instance id to get proper M matrix
    UNITY_TRANSFER_INSTANCE_ID(input, output);
    float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1)); // transform vertex to world position
    output.clipPos = mul(unity_MatrixVP, worldPos); // clup vertext to view projection
    return output;
}

float4 UnlitPassFragment(VertexOutput input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    return UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color);
}


#endif // MYRP_UNLIT_INCLUDED