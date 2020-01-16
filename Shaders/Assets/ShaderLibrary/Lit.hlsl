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
    float4 _VisibleLightDirectionsOrPositions[MAX_VISIBLE_LIGHTS]; // direction will be use for directional array and position will be used for point array
    float4 _VisibleLightAttenuations[MAX_VISIBLE_LIGHTS];
CBUFFER_END

// index - index of the light to use
// normal - surface normal
// worldPos - vertex in world position
float3 DiffuseLight(int index, float3 normal, float3 worldPos)
{
    float3 lightColor = _VisibleLightColors[index].rgb;
    float4 lightPositionOrDirection = _VisibleLightDirectionsOrPositions[index];
    float4 lightAttenuation = _VisibleLightAttenuations[index];
    
    float3 lightVector = lightPositionOrDirection.xyz - worldPos * lightPositionOrDirection.w; // if it is a position vector w will be 1 otherwise 0 
    float lightDirection = normalize(lightVector);
    float diffuse = saturate(dot(normal,lightDirection));
    
    float rangeFade = dot(lightVector, lightVector) * lightAttenuation.x;
    rangeFade = saturate(1.0 - rangeFade * rangeFade);
    rangeFade *= rangeFade;
    float distanceSqr = max(dot(lightVector, lightVector), 0.00001); // calculate the distance from vertex to light
	diffuse *= rangeFade / distanceSqr; // decrease intensity depending on distance and range fade
 
   
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
    float3 worldPos : TEXCOORD1; // vertex in world position
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
    output.worldPos = worldPos.xyz;
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
        diffuseLight += DiffuseLight(i, input.normal, input.worldPos);
    }
    
    float3 color = diffuseLight * albedo;
    
    return float4(color, 1);
}


#endif // MYRP_LIT_INCLUDED