Shader "Custom/Grid" 
{
	Properties
	{
		_LineColor("Line Color", Color) = (1,1,1,1)
		_CellColor("Cell Color", Color) = (0,0,0,0)
		_SelectedColor("Selected Color", Color) = (1,0,0,1)
		[PerRendererData] _MainTex("Albedo (RGB)", 2D) = "white" {}
		[HideInInspector] [IntRange] _GridSizeX("Grid Size X", Range(1, 100)) = 16
		[HideInInspector] [IntRange] _GridSizeY("Grid Size Y", Range(1, 100)) = 16
		[HideInInspector] _LineSize("Line Size", Range(0,1)) = 0.1 // I hid it because I think it is a pretty useless parameter
	}

	SubShader
	{
		Tags { "Queue" = "AlphaTest" "RenderType" = "TransparentCutout" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _CellData; // global variable

		sampler2D _MainTex;

		struct Input 
		{
			float2 uv_MainTex;
		};

		float4 _LineColor;
		float4 _CellColor;
		float4 _SelectedColor;

		float _GridSizeX;
		float _GridSizeY;
		float _LineSize;

		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input IN, inout SurfaceOutput o)
		{
			float2 uv = IN.uv_MainTex;
			fixed4 c = float4(0.0, 0.0, 0.0, 0.0);

			float brightness = 1.0;

			float2 id;
			id.x = floor((1.0 - uv.x) / (1.0 / _GridSizeX));
			id.y = floor((1.0 - uv.y) / (1.0 / _GridSizeY));
			
			float4 color = _CellColor;
			brightness = _CellColor.w;

			// sample selection texture data
			float4 data = tex2D(_CellData, 1.0 - uv);

			// This checks if the cell is currently selected
			if (data.r > 0.5)
			{
				brightness = _SelectedColor.w;
				color = _SelectedColor;
			}

			if (frac(uv.x * _GridSizeX) <= _LineSize || frac(uv.y * _GridSizeY) <= _LineSize)
			{
				brightness = _LineColor.w;
				color = _LineColor;
			}

			// Clip transparent spots using alpha cutout
			if (brightness == 0.0)
				clip(c.a - 1.0);

			o.Albedo = float4(color.x * brightness, color.y * brightness, color.z * brightness, brightness);
			o.Alpha = 0.0;
		}
		ENDCG
	}

	FallBack "Diffuse"
}
