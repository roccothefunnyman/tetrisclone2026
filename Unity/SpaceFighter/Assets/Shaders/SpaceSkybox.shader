// Space Skybox Shader - Procedural deep space background with subtle nebula
Shader "Skybox/SpaceSkybox"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (0.02, 0.02, 0.05, 1)
        _BottomColor ("Bottom Color", Color) = (0.0, 0.0, 0.02, 1)
        _NebulaColor1 ("Nebula Color 1", Color) = (0.1, 0.02, 0.15, 0.3)
        _NebulaColor2 ("Nebula Color 2", Color) = (0.02, 0.08, 0.12, 0.3)
        _NebulaScale ("Nebula Scale", Range(0.1, 10)) = 2
        _NebulaIntensity ("Nebula Intensity", Range(0, 1)) = 0.3
        _StarDensity ("Star Density", Range(0, 1000)) = 500
        _StarBrightness ("Star Brightness", Range(0, 2)) = 1
        _StarSize ("Star Size", Range(0.0001, 0.01)) = 0.002
    }

    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _NebulaColor1;
            fixed4 _NebulaColor2;
            float _NebulaScale;
            float _NebulaIntensity;
            float _StarDensity;
            float _StarBrightness;
            float _StarSize;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            // Hash function for procedural generation
            float hash(float3 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
            }

            // 3D noise function
            float noise(float3 p)
            {
                float3 i = floor(p);
                float3 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);

                return lerp(
                    lerp(
                        lerp(hash(i + float3(0,0,0)), hash(i + float3(1,0,0)), f.x),
                        lerp(hash(i + float3(0,1,0)), hash(i + float3(1,1,0)), f.x),
                        f.y),
                    lerp(
                        lerp(hash(i + float3(0,0,1)), hash(i + float3(1,0,1)), f.x),
                        lerp(hash(i + float3(0,1,1)), hash(i + float3(1,1,1)), f.x),
                        f.y),
                    f.z);
            }

            // Fractal noise for nebula
            float fbm(float3 p)
            {
                float value = 0.0;
                float amplitude = 0.5;

                for(int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(p);
                    p *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }

            // Star field function
            float stars(float3 dir)
            {
                float3 p = dir * _StarDensity;
                float3 i = floor(p);
                float3 f = frac(p);

                float star = 0.0;

                // Check neighboring cells
                for(int x = -1; x <= 1; x++)
                {
                    for(int y = -1; y <= 1; y++)
                    {
                        for(int z = -1; z <= 1; z++)
                        {
                            float3 cell = i + float3(x, y, z);
                            float3 cellCenter = cell + hash(cell) * 0.8 + 0.1;
                            float d = length(p - cellCenter);

                            // Star brightness varies
                            float brightness = hash(cell * 1.234) * _StarBrightness;

                            // Create star point
                            if(d < _StarSize * (1.0 + brightness))
                            {
                                star = max(star, brightness * (1.0 - d / (_StarSize * (1.0 + brightness))));
                            }
                        }
                    }
                }

                return saturate(star);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.vertex.xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float3 dir = normalize(i.worldPos);

                // Vertical gradient
                float t = dir.y * 0.5 + 0.5;
                fixed4 col = lerp(_BottomColor, _TopColor, t);

                // Add nebula clouds
                if(_NebulaIntensity > 0)
                {
                    float3 nebulaPos = dir * _NebulaScale;
                    float nebula1 = fbm(nebulaPos);
                    float nebula2 = fbm(nebulaPos + float3(100, 50, 25));

                    col.rgb += _NebulaColor1.rgb * nebula1 * _NebulaIntensity;
                    col.rgb += _NebulaColor2.rgb * nebula2 * _NebulaIntensity;
                }

                // Add stars
                float star = stars(dir);
                col.rgb += star;

                return col;
            }
            ENDCG
        }
    }
}
