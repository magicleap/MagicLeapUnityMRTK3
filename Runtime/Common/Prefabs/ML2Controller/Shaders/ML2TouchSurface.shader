// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2022-2024) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

Shader "Magic Leap/MRTK3/ML2TouchSurface"
{
    Properties
    {
        _GridColor ("Grid Color", Color) = (1,1,1,1)
        _PointColor ("Point Color", Color) = (1,1,1,1)
        _MainTex ("Touch Point Texture", 2D) = "white" {}
        _GridTex ("Touch Grid Texture", 2D) = "white" {}
        _FalloffTex ("Touch Grid Falloff Texture", 2D) = "white" {}
        _FalloffMult ("Touch Grid Falloff Multiplier", Range(0,1)) = .8
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            Cull back

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
                float2 uvPoint : TEXCOORD0;
                float2 uvGrid : TEXCOORD1;
                float2 uvFalloff : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            sampler2D _GridTex;
            sampler2D _FalloffTex;
            float4 _MainTex_ST;
            float4 _GridTex_ST;
            float4 _FalloffTex_ST;
            fixed4 _GridColor;
            fixed4 _PointColor;
            float _FalloffMult;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uvPoint = TRANSFORM_TEX(v.uv, _MainTex);
                o.uvGrid = TRANSFORM_TEX(v.uv, _GridTex);
                o.uvFalloff = TRANSFORM_TEX(v.uv, _FalloffTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 pointTex = tex2D(_MainTex, i.uvPoint);
                fixed4 gridTex = tex2D(_GridTex, i.uvGrid);
                fixed4 falloffTex = tex2D(_FalloffTex, i.uvFalloff);

                // Grid intensity modulated by falloff texture
                fixed4 gridIntensity = saturate(gridTex - ((1 - falloffTex) * _FalloffMult));
                fixed4 gridCol = gridIntensity * _GridColor;
                fixed4 pointCol = pointTex * _PointColor;

                fixed4 final = lerp(gridCol, pointCol, pointTex.a);

                return final;
            }
            ENDCG
        }
    }
}
