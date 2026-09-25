Shader "TOTALLED/DepthText"
{
    Properties { _MainTex("Font",2D)="white" {} _Color("Tint",Color)=(1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off ZTest LEqual
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; fixed4 _Color;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            Output vert(Input i) {Output o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv;o.color=i.color*_Color;return o;}
            fixed4 frag(Output i):SV_Target {fixed4 c=i.color;c.a*=tex2D(_MainTex,i.uv).a;return c;}
            ENDCG
        }
    }
}
