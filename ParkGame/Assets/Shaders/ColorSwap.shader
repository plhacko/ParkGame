Shader "Custom/ColorSwap"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _OriginalColor("Original Color", Color) = (1,1,1,1)
        _IgnoreColor("Ignore Color", Color) = (1,1,1,1)
        _TargetColor("Target Color", Color) = (1,1,1,1)
        _Tolerance("Tolerance", Range(0, 100)) = 0.001  
        _Darkness("Darkness", Range(0, 100)) = 0.001  
    }
 
    SubShader
    {
        Tags { "RenderType" = "Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
 
        Cull Off
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
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _OriginalColor;
            float4 _IgnoreColor;
            float4 _TargetColor;
            float _Tolerance;
            float _Darkness;
 
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
 
            half4 frag(v2f i) : SV_Target
            {
                half4 col = tex2D(_MainTex, i.uv);
 
                if (col.a == 0)
                {
                    return half4(0, 0, 0, 0);
                }
                
                if (length(col - _OriginalColor) < _Tolerance && length(col - _OriginalColor) < length(col - _IgnoreColor))
                {
                    return half4(_TargetColor.rgb * (col.r + col.g + col.b)/3 * _Darkness, col.a);
                }

                return col;
            }
 
            ENDCG
        }
 
 
    }
}
