Shader "Custom/VisionCircleShader"
{
    Properties
    {
        _Center ("Circle Center", Vector) = (0.5, 0.5, 0, 0) // Centre du cercle (x, y)
        _Radius ("Circle Radius", Float) = 0.3 // Rayon du cercle
        _Smoothness ("Smooth Edge", Float) = 0.1 // Lissage du bord
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _Center; // Centre du cercle
            float _Radius;  // Rayon du cercle
            float _Smoothness; // Lissage du bord

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv; // Pas besoin de texture dans ce cas
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                // Calcul de la distance par rapport au centre
                float dist = distance(uv, _Center.xy);
                // Création d'un bord lissé entre le cercle et le reste
                float alpha = 1.0 - smoothstep(_Radius - _Smoothness, _Radius, dist);
                // Fond noir et cercle transparent
                return fixed4(0, 0, 0, alpha);
            }
            ENDCG
        }
    }
}