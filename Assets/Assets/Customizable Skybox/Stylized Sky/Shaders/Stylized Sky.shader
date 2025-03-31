Shader "Stylized/Sky"
{
    Properties
    {
        [Header(Sun Disc)]
        _SunDiscColor ("Sun Color", Color) = (1, 1, 1, 1)
        _SunDiscMultiplier ("Sun Multiplier", float) = 50
        _SunDiscExponent ("Sun Exponent", float) = 1000

        [Header(Sun Halo)]
        _SunHaloColor ("Sun Halo Color", Color) = (0.8970588, 0.7760561, 0.6661981, 1)
        _SunHaloExponent ("Sun Halo Exponent", float) = 125
        _SunHaloContribution ("Sun Halo Contribution", Range(0, 1)) = 0.75

        [Header(Moon Disc)]
        _MoonDiscColor ("Moon Color", Color) = (0.8, 0.8, 1, 1)
        _MoonDiscMultiplier ("Moon Multiplier", float) = 50
        _MoonDiscExponent ("Moon Exponent", float) = 1000 

        [Header(Moon Halo)]
        _MoonHaloColor ("Moon Halo Color", Color) = (0.7, 0.7, 0.9, 1)
        _MoonHaloExponent ("Moon Halo Exponent", float) = 125
        _MoonHaloContribution ("Moon Halo Contribution", Range(0, 1)) = 0.75

        [Header(Horizon Line)]
        _HorizonLineColor ("Horizon Line Color", Color) = (0.9044118, 0.8872592, 0.7913603, 1)
        _HorizonLineExponent ("Horizon Line Exponent", float) = 4
        _HorizonLineContribution ("Horizon Line Contribution", Range(0, 1)) = 0.25
        
        [Header(Sky Gradient)]
        _SkyGradientTop ("Sky Gradient Top", Color) = (0.172549, 0.5686274, 0.6941177, 1)
        _SkyGradientBottom ("Sky Gradient Bottom", Color) = (0.764706, 0.8156863, 0.8509805)
        _SkyGradientExponent ("Sky Gradient Exponent", float) = 2.5
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Background"
            "Queue" = "Background"
        }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            
            float3 _SunDiscColor;
            float _SunDiscExponent;
            float _SunDiscMultiplier;

            float3 _SunHaloColor;
            float _SunHaloExponent;
            float _SunHaloContribution;

            float3 _MoonDiscColor;
            float _MoonDiscExponent;
            float _MoonDiscMultiplier;

            float3 _MoonHaloColor;
            float _MoonHaloExponent;
            float _MoonHaloContribution;

            float3 _HorizonLineColor;
            float _HorizonLineExponent;
            float _HorizonLineContribution;

            float3 _SkyGradientTop;
            float3 _SkyGradientBottom;
            float _SkyGradientExponent;

            float3 _SunDirection;
            float3 _MoonDirection;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPosition : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float maskHorizon = dot(normalize(i.worldPosition), float3(0, 1, 0));

                float maskSunDir = dot(normalize(i.worldPosition), _SunDirection);
                float maskSun = pow(saturate(maskSunDir), _SunDiscExponent);
                maskSun = saturate(maskSun * _SunDiscMultiplier);

                float3 sunHaloColor = _SunHaloColor * _SunHaloContribution;
                float sunBellCurve = pow(saturate(maskSunDir), _SunHaloExponent * saturate(abs(maskHorizon)));
                float sunHorizonSoften = 1 - pow(1 - saturate(maskHorizon), 50);
                sunHaloColor *= saturate(sunBellCurve * sunHorizonSoften);

                float maskMoonDir = dot(normalize(i.worldPosition), _MoonDirection);
                float maskMoon = pow(saturate(maskMoonDir), _MoonDiscExponent);
                maskMoon = saturate(maskMoon * _MoonDiscMultiplier);

                float3 moonHaloColor = _MoonHaloColor * _MoonHaloContribution;
                float moonBellCurve = pow(saturate(maskMoonDir), _MoonHaloExponent * saturate(abs(maskHorizon)));
                float moonHorizonSoften = 1 - pow(1 - saturate(maskHorizon), 50);
                moonHaloColor *= saturate(moonBellCurve * moonHorizonSoften);

                float3 horizonLineColor = _HorizonLineColor * saturate(pow(1 - abs(maskHorizon), _HorizonLineExponent));
                horizonLineColor = lerp(0, horizonLineColor, _HorizonLineContribution);

                float3 skyGradientColor = lerp(_SkyGradientTop, _SkyGradientBottom, pow(1 - saturate(maskHorizon), _SkyGradientExponent));

                float3 finalColor = saturate(sunHaloColor + moonHaloColor + horizonLineColor + skyGradientColor);
                finalColor = lerp(finalColor, _SunDiscColor, maskSun);
                finalColor = lerp(finalColor, _MoonDiscColor, maskMoon);

                return float4(finalColor, 1);
            }
            ENDCG
        }
    }
}