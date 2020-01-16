#ifndef MYRP_LIT_INCLUDED
#define MYRP_LIT_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl" // requires for CBUFFER_START and CBUFFER_END


// buffer the view projection matrix to only ger once per frame
CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END

// buffer the object to world matrix to only get it per draw
CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
CBUFFER_END

#define MAX_VISIBLE_LIGHTS 4

// buffer that holds reference to all lights, from source to light
CBUFFER_START(_LightBuffer)
    float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
    float4 _VisibleLightDirectionsOrPositions[MAX_VISIBLE_LIGHTS];
CBUFFER_END

float3 DiffuseLight(int index, float3 normal)
{
    float3 lightColor = _VisibleLightColors[index].rgb;
    float3 lightPositionOrDirection = _VisibleLightDirectionsOrPositions[index];
    float lightDirection = lightPositionOrDirection.xyz;
    float diffuse = saturate(dot(normal,lightDirection));
    return diffuse * lightColor;
}

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
    float3 normal : NORMAL; // surface normal (float3 is enough since we assume we use uniform scale)
    UNITY_VERTEX_INPUT_INSTANCE_ID // needed for GPU instancing to know which element from Ms array to take
};

struct VertexOutput
{
    float4 clipPos : SV_POSITION;
    float3 normal : TEXCOORD0; // surface normal
    UNITY_VERTEX_INPUT_INSTANCE_ID // needed for GPU instancing to know which color to take from array
};

VertexOutput LitPassVertex(VertexInput input)
{
    VertexOutput output;
    UNITY_SETUP_INSTANCE_ID(input); // set the instance id to get proper M matrix
    UNITY_TRANSFER_INSTANCE_ID(input, output); // Support for nonuniform scales would require us to use a transposed world-to-object matrix instead
    float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1)); // transform vertex to world position
    output.clipPos = mul(unity_MatrixVP, worldPos); // clup vertext to view projection
    output.normal = mul((float3x3) UNITY_MATRIX_M, input.normal);
    return output;
}

float4 LitPassFragment(VertexOutput input) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    input.normal = normalize(input.normal);
    float3 albedo = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color).rgb;
    
    float3 diffuseLight = 0;
    for (int i = 0; i < MAX_VISIBLE_LIGHTS; i++)
    {
        diffuseLight += DiffuseLight(i, input.normal);
    }
    
    float3 color = diffuseLight * albedo;
    
    return float4(color, 1);
}


#endif // MYRP_LIT_INCLUDED