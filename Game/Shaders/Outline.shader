Shader "Custom/PixelOutlineExtended"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineSize ("Outline Size", Range(1, 5)) = 1
        _EdgeDetection ("Edge Detection", Range(0.01, 0.5)) = 0.1
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
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _OutlineColor;
            float _OutlineSize;
            float _EdgeDetection;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                return o;
            }

            // Функция для безопасной проверки текстуры с учетом границ
            fixed4 SampleTextureSafe(sampler2D tex, float2 uv)
            {
                // Если UV выходят за пределы [0,1], считаем пиксель прозрачным
                if (uv.x < 0.0 || uv.x > 1.0 || uv.y < 0.0 || uv.y > 1.0)
                    return fixed4(0, 0, 0, 0);
                
                return tex2D(tex, uv);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 pixelOffset = _MainTex_TexelSize.xy * _OutlineSize;
                
                // Текущий пиксель
                fixed4 current = tex2D(_MainTex, i.texcoord);
                
                // Если текущий пиксель непрозрачный - возвращаем оригинальный цвет
                if (current.a > _EdgeDetection)
                {
                    return current;
                }
                
                // Проверяем все соседние пиксели (включая за пределами текстуры)
                fixed4 up = SampleTextureSafe(_MainTex, i.texcoord + float2(0, pixelOffset.y));
                fixed4 down = SampleTextureSafe(_MainTex, i.texcoord + float2(0, -pixelOffset.y));
                fixed4 left = SampleTextureSafe(_MainTex, i.texcoord + float2(-pixelOffset.x, 0));
                fixed4 right = SampleTextureSafe(_MainTex, i.texcoord + float2(pixelOffset.x, 0));
                
                fixed4 upLeft = SampleTextureSafe(_MainTex, i.texcoord + float2(-pixelOffset.x, pixelOffset.y));
                fixed4 upRight = SampleTextureSafe(_MainTex, i.texcoord + float2(pixelOffset.x, pixelOffset.y));
                fixed4 downLeft = SampleTextureSafe(_MainTex, i.texcoord + float2(-pixelOffset.x, -pixelOffset.y));
                fixed4 downRight = SampleTextureSafe(_MainTex, i.texcoord + float2(pixelOffset.x, -pixelOffset.y));
                
                // Если любой соседний пиксель непрозрачный - рисуем обводку
                if (up.a > _EdgeDetection || down.a > _EdgeDetection || 
                    left.a > _EdgeDetection || right.a > _EdgeDetection ||
                    upLeft.a > _EdgeDetection || upRight.a > _EdgeDetection || 
                    downLeft.a > _EdgeDetection || downRight.a > _EdgeDetection)
                {
                    return _OutlineColor;
                }
                
                return fixed4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}