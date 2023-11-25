Shader "Unlit/Revealer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Stencil
        {
            Ref 1
            Comp Always
            Pass Replace
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            
            fixed4 _Color;
            float4 _RevealersPositions[512];
            float _RevealersRadii[512];
            int _RevealersCount;
            int _PixelsPerUnit;
            int _Width;
            int _RadiusEdge;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                const fixed2 uv = floor((i.uv - 0.5f) * _Width * _PixelsPerUnit + 0.5f);

                const int radius = _RevealersRadii[0];
                const fixed2 position = _RevealersPositions[0].xy * _PixelsPerUnit;
                
                const fixed dist = max(0, distance(position, uv) - radius);
                const float distanceFraction = dist / _RadiusEdge;
                
                clip(1 - distanceFraction);
                
                return fixed4(0, 0, 0, 0);
                
                // for (int j = 0; j < _RevealersCount; j++)
                // {
                //     float4 pos = _RevealersPositions[j];
                //     float dist = length(pos.xy - i.uv);
                //     // float alpha = saturate(1.0 - dist / pos.w);
                //     col.rgb = dist;
                //     // col.a = lerp(col.a, 0, alpha);
                // }
                //
                // return col;
            }
            ENDCG
        }
    }
}
