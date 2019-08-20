Shader "PDT Shaders/TestGrid" 
{
	Properties
	{
		_LineColor("Line Color", Color) = (1,1,1,1)
		_CellColor("Cell Color", Color) = (0,0,0,0)
		_SelectedColor("Selected Color", Color) = (1,0,0,1)
		[PerRendererData] _MainTex("Albedo (RGB)", 2D) = "white" {}
		[IntRange] _GridSize("Grid Size", Range(1,100)) = 10
		_LineSize("Line Size", Range(0,1)) = 0.15
		_SelectedArea("Selected Area", Vector) = (-1,-1,-1,-1)
	}

	SubShader
	{
		Tags { "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		half _Glossiness = 0.0;
		half _Metallic = 0.0;
		float4 _LineColor;
		float4 _CellColor;
		float4 _SelectedColor;

		float _GridSize;
		float _LineSize;

		int4 _SelectedArea;

		UNITY_INSTANCING_BUFFER_START(Props)

		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutputStandard o)
		{
			float2 uv = IN.uv_MainTex;
			fixed4 c = float4(0.0, 0.0, 0.0, 0.0);

			float brightness = 1.0;
			
			float gsize = floor(_GridSize);
			float2 id;
			id.x = floor((1.0 - uv.x) / (1.0 / gsize));
			id.y = floor((1.0 - uv.y) / (1.0 / gsize));
			
			float4 color = _CellColor;
			brightness = _CellColor.w;

			// This checks that the cell is currently selected
			if (id.x >= _SelectedArea.x && id.y >= _SelectedArea.y && id.x <= _SelectedArea.z && id.y <= _SelectedArea.w)
			{
				brightness = _SelectedColor.w;
				color = _SelectedColor;
			}

			if (frac(uv.x * gsize) <= _LineSize || frac(uv.y * gsize) <= _LineSize)
			{
				brightness = _LineColor.w;
				color = _LineColor;
			}

			// Clip transparent spots using alpha cutout
			if (brightness == 0.0)
			{
				clip(c.a - 1.0);
			}

			o.Albedo = float4(color.x * brightness,color.y * brightness,color.z * brightness,brightness);
			// Metallic and smoothness come from slider variables
			o.Metallic = 0.0;
			o.Smoothness = 0.0;
			o.Alpha = 0.0;
		}
		ENDCG
	}

	FallBack "Diffuse"
}
