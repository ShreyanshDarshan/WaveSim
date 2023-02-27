Shader "Compute/Simulator"
{
    Properties
    {
        _MainTex ("MainTexture", 2D) = "white" {}
        // _SetupTex ("SetupTexture", 2D) = "white" {}
        _SpringForce ("SpringForce", Float) = 0.1
        _DampingFactor ("DampingFactor", Float) = 0.1
        [MaterialToggle] _IsPaused ("IsPaused", Float) = 1
        _MousePos ("MousePos", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Speed;
            float _IsPaused;
            float4 _MousePos;
            float _SpringForce;
            float _DampingFactor;

            float get_restoration_force(float2 uv_pos, float position) {
                float force = 0;
                float self_restoration_force = -position * _SpringForce;
                // force += self_restoration_force;
                [unroll]
                for (int i = -1; i <= 1; i++) {
                    [unroll]
                    for (int j = -1; j <= 1; j++) {
                        // Simulate self restoration
                        float2 neighbor = uv_pos + float2(i, j) * _MainTex_TexelSize.xy;
                        float4 col = tex2D(_MainTex, neighbor);
                        float neighbor_position = col.r;
                        float neighbor_restoration_force = -(position - neighbor_position) * _SpringForce;
                        force += neighbor_restoration_force * (1 - abs(i*j));
                        force += neighbor_restoration_force * abs(i*j) * 0.293;
                    }
                }
                return force;
            }

            float4 frag (v2f i) : SV_Target
            {
                if (i.uv.x < _MainTex_TexelSize.x || 
                    i.uv.x > 1 - _MainTex_TexelSize.x ||
                    i.uv.y < _MainTex_TexelSize.y ||
                    i.uv.y > 1 - _MainTex_TexelSize.y) {
                    return float4(0.0, 0.0, 0.0, 1);
                }
                // Simulate springs
                float4 color = tex2D(_MainTex, i.uv);
                float position = color.r;
                float velocity = color.g;
                if (_IsPaused == 0) {
                    float restoration_force = get_restoration_force(i.uv, position);
                    velocity += restoration_force;
                    float damping_force = -velocity * _DampingFactor;
                    velocity += damping_force;
                    position += velocity;
                }

                // Draw using mouse
                if (_MousePos.z == 1) {
                    float2 mouse_pos = _MousePos.xy;
                    float2 diff = abs(mouse_pos - i.uv * _MainTex_TexelSize.zw);
                    if (diff.x < 0.5 && diff.y < 0.5) {
                        position = 1;
                        velocity = 0;
                    }
                }

                return float4(position, velocity, -position, 1);
            }
            ENDCG
        }
    }
}
