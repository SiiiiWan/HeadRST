Shader "Custom/ThreeLayerRing"
{
    Properties
    {
        _ColorWhite ("White Color", Color) = (1,1,1,1)
        _ColorGray ("Gray Color", Color) = (0.5,0.5,0.5,1)
        _RingWidth ("White Ring Width", Float) = 0.05
        _OuterRadius ("Outer Radius", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        // 关键：支持 UI Mask 的 Stencil 配置
        Stencil
        {
            Ref 1
            Comp Equal
            Pass Keep
        }

        Pass
        {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            fixed4 _ColorWhite;
            fixed4 _ColorGray;
            float _RingWidth;
            float _OuterRadius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv - float2(0.5, 0.5);
                float dist = length(uv);

                float r_outer = _OuterRadius;
                float w = _RingWidth;
                float r_gray_outer = r_outer - w;
                float r_gray_inner = r_outer - 3.0 * w;
                float r_inner = r_outer - 4.0 * w;

                if (dist <= r_outer && dist > r_gray_outer)
                    return _ColorWhite;
                else if (dist <= r_gray_outer && dist > r_gray_inner)
                    return _ColorGray;
                else if (dist <= r_gray_inner && dist > r_inner)
                    return _ColorWhite;
                else
                    return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}