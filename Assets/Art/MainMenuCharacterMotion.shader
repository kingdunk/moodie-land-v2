Shader "MoodieLand/Main Menu Character Motion"
{
    Properties
    {
        [PerRendererData] _MainTex ("Standing Sprite", 2D) = "white" {}
        _PoseTex ("Gesture Sprite", 2D) = "white" {}
        _Character ("Character", Float) = 0
        _Motion ("Motion", Vector) = (0, 0, 0, 0)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            sampler2D _PoseTex;
            float _Character;
            float4 _Motion;
            float4 _ClipRect;

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float2 localPosition : TEXCOORD1;
                fixed4 color : COLOR;
            };

            Varyings Vert(appdata_full vertex)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(vertex.vertex);
                output.uv = vertex.texcoord.xy;
                output.localPosition = vertex.vertex.xy;
                output.color = vertex.color;
                return output;
            }

            // The source art is full-rect, 480 by 480, with top-left coordinates.
            fixed4 SampleArt(sampler2D art, float2 pixel)
            {
                fixed4 color = tex2D(art, float2(pixel.x, 480.0 - pixel.y) / 480.0);
                color.rgb *= color.a;
                return color;
            }

            float InsideEllipse(float2 pixel, float2 center, float2 radius)
            {
                return 1.0 - smoothstep(0.80, 1.0, length((pixel - center) / radius));
            }

            fixed4 Pip(float2 pixel)
            {
                float paw = InsideEllipse(pixel, float2(355.0, 260.0), float2(52.0, 47.0))
                    * smoothstep(326.0, 359.0, pixel.x)
                    * smoothstep(236.0, 253.0, pixel.y);
                float scarf = InsideEllipse(pixel, float2(204.0, 293.0), float2(39.0, 36.0))
                    * (1.0 - smoothstep(197.0, 221.0, pixel.x));
                float2 offset = _Motion.x * (float2(5.0, -9.0) * paw + float2(-2.0, 3.0) * scarf);
                return lerp(SampleArt(_PoseTex, pixel), SampleArt(_MainTex, pixel - offset), _Motion.w);
            }

            fixed4 Mimi(float2 pixel)
            {
                // Fade the displacement to zero along the overlap with the cheeks and torso.
                float boundary = lerp(274.0, 262.0, smoothstep(260.0, 392.0, pixel.y));
                float tail = smoothstep(boundary, boundary + 67.0, pixel.x)
                    * smoothstep(121.0, 156.0, pixel.y)
                    * (1.0 - smoothstep(330.0, 420.0, pixel.y));
                return SampleArt(_MainTex, pixel - float2(10.0, -8.0) * (_Motion.x * tail));
            }

            fixed4 Bobo(float2 pixel)
            {
                float ears = 1.0 - smoothstep(72.0, 145.0, pixel.y);
                pixel -= float2(4.0, 3.0) * (_Motion.y * ears);
                fixed4 standing = SampleArt(_MainTex, pixel);
                float arm = smoothstep(219.0, 230.0, pixel.y)
                    * (1.0 - smoothstep(342.0, 360.0, pixel.y))
                    * (1.0 - smoothstep(227.0, 243.0, pixel.x));
                float paw = 1.0 - smoothstep(170.0, 200.0, pixel.x);
                fixed4 waving = SampleArt(_PoseTex, pixel - float2(4.0, -3.0) * (_Motion.z * paw));
                return lerp(standing, waving, arm * _Motion.x);
            }

            fixed4 Nunu(float2 pixel, fixed4 standing)
            {
                float leftEye = InsideEllipse(pixel, float2(179.0, 185.0), float2(18.0, 19.0));
                float rightEye = InsideEllipse(pixel, float2(260.0, 205.0), float2(18.0, 19.0));
                float mouth = InsideEllipse(pixel, float2(213.0, 215.0), float2(30.0, 21.0));
                return lerp(standing, SampleArt(_PoseTex, pixel),
                    saturate((leftEye + rightEye) * _Motion.x + mouth * _Motion.y));
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                float2 pixel = float2(input.uv.x, 1.0 - input.uv.y) * 480.0;
                fixed4 color = SampleArt(_MainTex, pixel);
                if (_Character < 0.5)
                    color = Pip(pixel);
                else if (_Character < 1.5)
                    color = Mimi(pixel);
                else if (_Character < 2.5)
                    color = Bobo(pixel);
                else
                    color = Nunu(pixel, color);

                color *= input.color.a;
                color.rgb *= input.color.rgb;
                #ifdef UNITY_UI_CLIP_RECT
                    color *= UnityGet2DClipping(input.localPosition, _ClipRect);
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                #endif
                return color;
            }
            ENDCG
        }
    }
}
