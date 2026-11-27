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

            fixed4 Wave(float2 pixel, fixed4 standing, bool pip)
            {
                // The two gesture sprites include full expressions. Restrict the
                // composite to the near arm and its attachment to the chest.
                float y = pixel.y;
                float shoulder = smoothstep(214.0, 315.0, y);
                float seam = pip ? lerp(139.0, 220.0, shoulder) : lerp(157.0, 221.0, shoulder);
                float lower = smoothstep(309.0, 365.0, y);
                seam += (pip ? 17.0 : 9.0) * lower;
                float rearFoot = pip ? 178.0 : 193.0;
                float keepRearFoot = smoothstep(354.0, 393.0, y)
                    * (1.0 - smoothstep(rearFoot - 8.0, rearFoot + 8.0, pixel.x));
                float mask = (1.0 - smoothstep(seam - 5.0, seam + 5.0, pixel.x))
                    * smoothstep(pip ? 186.0 : 222.0, pip ? 196.0 : 235.0, y)
                    * (1.0 - keepRearFoot);

                float2 palm = pip ? float2(135.0, 234.0) : float2(133.0, 261.0);
                float wrist = InsideEllipse(pixel, palm, float2(69.0, 84.0));
                float2 source = pixel - float2(pip ? 34.0 : 3.0, 0.0);
                source -= float2(9.0, -4.0) * (_Motion.y * wrist);
                return lerp(standing, SampleArt(_PoseTex, source), mask * _Motion.x);
            }

            fixed4 Mimi(float2 pixel)
            {
                // Taper motion to zero where the tail meets the face and body.
                float boundary = lerp(327.0, 298.0, smoothstep(324.0, 403.0, pixel.y));
                float tail = smoothstep(boundary, boundary + 75.0, pixel.x)
                    * (1.0 - smoothstep(320.0, 421.0, pixel.y));
                float2 offset = float2(20.0, -17.0) * (_Motion.x * tail);

                // Center the gaze inside the two existing eye openings.
                offset += float2(-8.0, 6.0) * InsideEllipse(pixel, float2(199.0, 195.0), float2(27.0, 30.0));
                offset += float2(-3.0, 5.0) * InsideEllipse(pixel, float2(290.0, 174.0), float2(24.0, 25.0));
                return SampleArt(_MainTex, pixel - offset);
            }

            fixed4 Nunu(float2 pixel, fixed4 standing)
            {
                float leftEye = InsideEllipse(pixel, float2(180.0, 204.0), float2(34.0, 36.0))
                    * smoothstep(172.0, 180.0, pixel.y);
                float rightEye = InsideEllipse(pixel, float2(260.0, 201.0), float2(35.0, 36.0))
                    * smoothstep(168.0, 177.0, pixel.y);
                fixed4 face = lerp(standing, SampleArt(_PoseTex, pixel + float2(-3.0, 3.0)), leftEye * _Motion.x);
                face = lerp(face, SampleArt(_PoseTex, pixel + float2(3.0, -1.0)), rightEye * _Motion.x);
                float mouth = InsideEllipse(pixel, float2(221.0, 240.0), float2(25.0, 12.0));
                return lerp(face, SampleArt(_PoseTex, pixel + float2(8.0, 0.0)), mouth * _Motion.y);
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                float2 pixel = float2(input.uv.x, 1.0 - input.uv.y) * 480.0;
                fixed4 color = SampleArt(_MainTex, pixel);
                if (_Character < 0.5)
                    color = Wave(pixel, color, true);
                else if (_Character < 1.5)
                    color = Mimi(pixel);
                else if (_Character < 2.5)
                    color = Wave(pixel, color, false);
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
