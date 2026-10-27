// Wymusza dokladnie 2 kolory: prog luminancji + maska mgly (cena "Mgla").
Shader "Jam/TwoColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Bg ("Background", Color) = (0.08, 0.07, 0.1, 1)
        _Fg ("Foreground", Color) = (0.95, 0.95, 0.87, 1)
        _FogCenter ("Fog center (uv)", Vector) = (0.5, 0.5, 0, 0)
        _FogRadius ("Fog radius (in screen heights, 0 = off)", Float) = 0
        _Aspect ("Aspect", Float) = 1.7778
        _Threshold ("Threshold", Float) = 0.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Cull Off ZWrite Off ZTest Always Blend Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Bg;
            fixed4 _Fg;
            float4 _FogCenter;
            float _FogRadius;
            float _Aspect;
            float _Threshold;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed3 c = tex2D(_MainTex, i.uv).rgb;
                float lum = dot(c, float3(0.299, 0.587, 0.114));
                bool lit = lum > _Threshold;

                if (_FogRadius > 0)
                {
                    float2 d = (i.uv - _FogCenter.xy) * float2(_Aspect, 1);
                    if (dot(d, d) > _FogRadius * _FogRadius) lit = false;
                }
                return lit ? _Fg : _Bg;
            }
            ENDCG
        }
    }
}
