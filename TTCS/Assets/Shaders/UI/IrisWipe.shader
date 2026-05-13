Shader "UI/IrisWipe"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (0,0,0,1)
        _Radius ("Radius", Range(0, 1.2)) = 1
        _Softness ("Softness", Range(0, 0.2)) = 0.03
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
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            fixed4 _Color;
            sampler2D _MainTex;
            float _Radius;
            float _Softness;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                float2 d = i.uv - float2(0.5, 0.5);
                float dist = length(d) * 2.0; // center=0, circle touches edges ~=1

                float edge0 = _Radius;
                float edge1 = _Radius - max(_Softness, 0.0001);

                // alpha=0 inside hole, alpha=1 outside
                float alpha = smoothstep(edge1, edge0, dist);

                fixed4 col = tex * i.color;
                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}
