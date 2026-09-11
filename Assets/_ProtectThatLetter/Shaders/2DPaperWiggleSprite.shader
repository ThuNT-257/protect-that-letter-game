Shader "Custom/2DPaperWiggleSprite"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        [Header(Paper Wave Settings)]
        _Speed ("Wave Speed", Range(0, 20)) = 6.0
        _Frequency ("Wave Frequency", Range(0, 30)) = 8.0
        _Amount ("Wave Distortion Amount", Range(0, 0.1)) = 0.02
        
        [Header(Paper Flutter Corner Settings)]
        _FlutterSpeed ("Corner Flutter Speed", Range(0, 30)) = 12.0
        _FlutterAmount ("Corner Flutter Amount", Range(0, 0.05)) = 0.015

        [HideInInspector] _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
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

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float _Speed;
            float _Frequency;
            float _Amount;
            float _FlutterSpeed;
            float _FlutterAmount;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                
                float mainWave = sin(_Time.y * _Speed + (IN.vertex.x + IN.vertex.y) * _Frequency) * _Amount;
                
                float distanceFromCenter = length(IN.texcoord - float2(0.5, 0.5));
                float cornerFlutter = cos(_Time.y * _FlutterSpeed + IN.vertex.x * 20.0) * _FlutterAmount * distanceFromCenter;

                IN.vertex.x += mainWave;
                IN.vertex.y += cornerFlutter;

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
                c.rgb *= c.a;
                return c;
            }
            ENDCG
        }
    }
}