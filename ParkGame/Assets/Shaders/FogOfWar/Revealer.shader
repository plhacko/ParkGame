Shader "Unlit/Revealer"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags 
        { 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

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

                fixed minDist = 100000000;
                for (int j = 0; j < _RevealersCount; j++)
                {
                    const int radius = _RevealersRadii[j];
                    const fixed2 position = _RevealersPositions[j].xy * _PixelsPerUnit;
                
                    minDist = min(minDist, distance(position, uv) - radius);
                }
                    
                clip(1 - minDist / _RadiusEdge);
                
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
