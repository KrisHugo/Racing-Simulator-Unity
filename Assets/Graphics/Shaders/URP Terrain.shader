Shader "Custom/URP Terrain"
{
    Properties
    {
        // _BaseColor ("Color", Color) = (1, 1, 1, 1) // 默认白色
        _minHeight ("MinHeight", Float) = 0.0 // 最小高度
        _maxHeight ("MaxHeight", Float) = 0.0 // 最小高度
        

    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
        }
        
        Pass
        {
            Name "UnlitPass"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            // 包含URP核心库
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            // 声明属性变量
            // half4 _BaseColor;
            CBUFFER_START(UnityPerMaterial)
                float _minHeight;
                float _maxHeight;
            CBUFFER_END
            
            // 顶点着色器输入结构
            struct Attributes
            {
                float4 positionOS : POSITION; // 物体空间位置
                
            };
            
            // 顶点着色器输出结构
            struct Varyings
            {
                float4 positionCS : SV_POSITION; // 裁剪空间位置
                float3 positionWS : TEXCOORD0; // 世界空间位置
            };

            float inverseLerp(float a, float b, float value)
            {
                return saturate((value - a) / (b - a));
            }
            
            // 顶点着色器
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                // 转换到裁剪空间
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz); // 世界空间位置
                
                return output;
            }
            
            // 片段着色器
            half4 frag(Varyings input) : SV_Target
            {
                float heightPercent = inverseLerp(_minHeight, _maxHeight, input.positionWS.y); 
                half3 color = lerp(half3(0, 0, 0), half3(1, 1, 1), heightPercent);

                // 直接返回固定颜色
                return half4(color, 1.0);
            
            }
            
            // 声明顶点和片段着色器
            #pragma vertex vert
            #pragma fragment frag
            
            ENDHLSL
        }
    }
}