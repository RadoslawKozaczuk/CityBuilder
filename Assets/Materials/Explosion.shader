Shader "Custom/Explosion"
{
	// volumetric explosion works in two steps.
	// 1) vertex function changes the geometry
	// 2) surface gives it the right color
    Properties
    {
		_RampTex("Color Ramp", 2D) = "white" {}
		_RampOffset("Ramp offset", Range(-0.5, 0.5)) = 0 // negative values means more fire, positive more smoke
		_NoiseTex("Noise Texture", 2D) = "grey" {}
		_Amount("Amount", Range(0.7, 0.9)) = 0.8
		_ClipRange("ClipRange", Range(0, 1)) = 1
		_TimeOffset("TimeOffset", Range(0, 10)) = 0 
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        // Lambert lighting model because it's the cheapest one
        #pragma surface surf Lambert vertex:vert nolightmap

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

		// for debug only
		//#pragma enable_d3d11_debug_symbols

		// we need to add all the variables so the CG code can actually access them
        sampler2D _RampTex;
		half _RampOffset;
		sampler2D _NoiseTex;
		half _Amount;
		half _ClipRange;
		half _TimeOffset;

        struct Input
        {
            float2 uv_NoiseTex; // we need uv from the noise texture
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        
		// put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata_full v)
		{
			// there is no standard way to get a random number in a shader, so the easiest way is to sample a noise texture
			float3 disp = tex2Dlod(_NoiseTex, float4(v.texcoord.xy, 0, 0));

			// _Time variable stores time since level load (t/20, t, t*2, t*3)
			float totalTime = _Time[1] - _TimeOffset;
			// sinus function makes the vertices go up and down, simulating the chaotic behavior of a real explosion
			float time = sin(totalTime + disp.r * 12);
			
			v.vertex.xyz += v.normal * disp.r * _Amount * time;
		}
		
        void surf(Input IN, inout SurfaceOutput o)
        {
			float3 noise = tex2D(_NoiseTex, IN.uv_NoiseTex);
			
			// saturate returns x saturated to the range [0,1] as follows:
			// returns 0 if x is less than 0
			// otherwise returns 1 if x is greater than 1
			// otherwise returns x
			float n = saturate(noise.r + _RampOffset);

			clip(_ClipRange - n);
			half4 c = tex2D(_RampTex, float2(n, 0.5));

			o.Albedo = c.rgb;
			o.Emission = c.rgb * c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
