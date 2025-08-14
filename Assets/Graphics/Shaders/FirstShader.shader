Shader "Custom/FirstShader"
{
    Properties
    {
        [HDR]_Color("Base Color", Color) = (1,1,1,1)
        _ClipStrength("Clip Strength", Float) = 10
        _DirtColor("Dirt Color", Color) = (0.5,0,0.5,1)
        [PowerSlider(1)]_DirtStrength("Dirt Strength", Range(0, 1)) = 0.5
        [Toggle]_DirtedEnabled("Dirted Enabled", Range( 0 , 1)) = 1

        _FrontTex("FrontTex", 2d) = "white"{}
		_BackTex("BackTex", 2d) = "white"{}
    }
    SubShader
    {
        cull off
        Pass
        {
            CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			sampler2D _FrontTex;
			sampler2D _BackTex;
            
            fixed4 _Color;
            float _ClipStrength;

            struct appdata
            {
                float4 vertex : POSITION;		//顶点
                float4 tangent : TANGENT;		//切线
                float3 normal : NORMAL;			//法线
                float4 texcoord : TEXCOORD0;	        //UV1
                float4 texcoord1 : TEXCOORD1;	        //UV2
                float4 texcoord2 : TEXCOORD2;	        //UV3
                float4 texcoord3 : TEXCOORD3;	        //UV4
                fixed4 color : COLOR;			//顶点色
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos( v.vertex );
                o.uv = v.texcoord;
                return o;
            }

            fixed checker(float2 uv)
            {
                float2 repeatUV = uv * _ClipStrength;
                float2 c = floor(repeatUV) / 2;
                float checker = frac(c.x + c.y) * 2;
                return checker;
            }

            fixed4 frag (v2f i, float face:VFACE) : SV_TARGET
            {
                // fixed color = checker(i.uv);
                // return color;
                fixed4 col=1;
				col = face > 0 ? tex2D(_FrontTex,i.uv) : checker(i.uv);
				return col;
            }

			ENDCG
        }
    }

    FallBack "Diffuse"
    // CustomEditor "EditorName"
}
