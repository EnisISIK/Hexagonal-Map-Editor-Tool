Shader "Hex/WaterShader" {

	Properties {
		_MainTex ("First Texture", 2D) = "white" {}
		_SecondaryTex("Second Texture", 2D) = "white" {}
	}

	SubShader {
		
		Tags {"Queue"="Transparent" "RenderType"="Transparent"}
		LOD 100
		Lighting Off
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass {
		
			CGPROGRAM
				#pragma vertex vertFunction
				#pragma fragment fragFunction
				#pragma target 2.0

				#include "UnityCG.cginc"

				struct appdata {
				
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;

				};

				struct v2f {
				
					float4 vertex : SV_POSITION;
					float2 uv : TEXCOORD0;
					float4 color : COLOR;

				};

				sampler2D _MainTex;
				sampler2D _SecondaryTex;

				v2f vertFunction (appdata v) {
				
					v2f o;

					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					o.color = v.color;

					return o;

				}

				fixed4 fragFunction (v2f i) : SV_Target {
				
					i.uv.x += (_SinTime.x * 0.7);

					fixed4 texture1 = tex2D(_MainTex, i.uv);
					fixed4 texture2 = tex2D(_SecondaryTex, i.uv);

					fixed4 color1 = lerp(texture1, texture2, 0.5 + (_SinTime.w * 0.5));

					clip(color1.a - 0.2);
					color1.a = 0.7f;

					return color1;

				}

				ENDCG

		}


	}

}