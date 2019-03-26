Shader "Example/MyShader Lit"
{
	Properties
	{
		_MyColor("My colour", Color) = (1, 0, 0, 1) // (R, G, B, A)
		_MySmoothness("Smoothness", Range(0.0,1.0)) = 0.5
	}

	SubShader
	{

		CGPROGRAM
		#pragma surface surf Standard
		struct Input {
			float4 color : COLOR;
		};
		half4 _MyColor;
		half _MySmoothness;

		void surf(Input IN, inout SurfaceOutputStandard o) {
			o.Albedo = _MyColor;
			o.Smoothness = _MySmoothness;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
