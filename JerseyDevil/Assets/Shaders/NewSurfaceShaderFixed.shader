
Shader "Unlit/WorldspaceTilingFixed"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Transform vertex position to world space
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                
                // Remove rotation by transforming world position using only the scale
                float3 scaleOnly = float3(length(unity_ObjectToWorld._m00_m10_m20),
                                          length(unity_ObjectToWorld._m01_m11_m21),
                                          length(unity_ObjectToWorld._m02_m12_m22));
                
                float3 scaledWorldPos = worldPos / scaleOnly;

                // Use scaled world-space coordinates for consistent tiling
                o.uv = scaledWorldPos.xy * _MainTex_ST.xy + _MainTex_ST.zw;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {                   
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
