Shader "Example/MyShader"
{
	Properties
	{
		_MyColor("My colour", Color) = (1, 0, 0, 1) // (R, G, B, A)
	}
		SubShader
	{
		ZTest Off
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			half4 _MyColor;


			struct v2f {
				float4 pos : SV_POSITION;
			};
			v2f vert(appdata_base v)
			{
				v2f o;
				//UNITY_MATRIX_MVP is a macro for a matrix to be injected pre compilation
				//mul(UNITY_MATRIX_MVP, float4(pos, 1.0));
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}
			half4 frag(v2f output) : SV_TARGET
			{
				return _MyColor;
			}
			ENDCG
		}
	}
		FallBack "Diffuse"
}
