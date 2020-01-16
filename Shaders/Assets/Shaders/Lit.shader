Shader "MyPipeline/Lit"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
                
            // make shader only targetable non OpernGLES 2 devices (to enable OpenGLES2 use #pragma prefer_hlslcc gles)    
            #pragma target 3.5
            
            // inform unity that we only use uniform scale, other wise if we are not we will have to add
            // default world-to-object matrices are put in the instancing buffer. Those are the inverse of the M matrices, 
            // which are needed for normal vectors when using non-uniform scales.
            #pragma instancing_options assumeuniformscaling
            
            // allow GPU instancing 
            #pragma multi_compile_instancing
                
            // define UnlitPassVertex for vertex function
            #pragma vertex LitPassVertex
            
            // define UnlitPassFragment for fragment funtion
            #pragma fragment LitPassFragment
            
            #include "../ShaderLibrary/Lit.hlsl"
                
            ENDHLSL
        }
    }
}
