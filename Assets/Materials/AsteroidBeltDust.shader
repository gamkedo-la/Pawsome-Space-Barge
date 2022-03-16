Shader "Custom/AsteroidBeltDust"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _DustStart ("Dust Minimum Opacity Depth", Range(0,1)) = 0.0
        _DustEnd ("Dust Maximum Opacity Depth", Range(0,1)) = 0.5
        _DustMinOpacity ("Minimum Opacity", Range(0,1)) = 0
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent" "RenderType"="Transparent"
        }
        LOD 200
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Lambert alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        #pragma shader_feature _ _PLAY_MODE

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        fixed4 _Color;
        float _DustStart;
        float _DustEnd;
        float _DustMinOpacity;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
        // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        
        void surf(Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;

            // Calculate depth for dust alpha
            float4 clipPos = UnityWorldToClipPos(IN.worldPos);
            float3 normScreenPos = clipPos.xyz/clipPos.w;
            // Difference between OpenGL and DirectX
            #if (defined(SHADER_API_GLES) || defined(SHADER_API_GLES3))
                float depth = Linear01Depth((normScreenPos.z + 1.0f) * 0.5f);                             
            #else
                float depth = Linear01Depth(normScreenPos.z);
            #endif

            
            #if defined(_PLAY_MODE)
                float dustFactor = lerp(_DustMinOpacity, 1, saturate(1.0 - (_DustEnd - depth)/(_DustEnd - _DustStart))); 
            #else
                float dustFactor = 1;
            #endif

            o.Alpha = c.a * dustFactor;
        }

        ENDCG
    }
    FallBack "Diffuse"
}