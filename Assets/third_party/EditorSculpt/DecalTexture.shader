// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DecalTexture"
	{
		Properties
		{
			_MainTex("Decal Texture", 2D) = "white" {}
			_DecalCam("Decal Camera", Vector) = (0,0,0,0)
			_DecalPos("Decal Position", Vector) = (0,0,0,0)
			_DecalPos2("Decal Position2", Vector) = (0,0,0,0)
			_DecalAxis("Decal Axis",Int) = 0
			_DecalDeform("Decal Deform",Int)=0
			_Transparent("Decal Transparent",Float) = 1
		}
	
    	SubShader
    	{
			//Add v1.16 for transparent
			Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
			//
       	 Pass
        	{
				//Add v1.16 for tranparent
				ZWrite Off
				Blend SrcAlpha OneMinusSrcAlpha
				//
        		CGINCLUDE
				#include "UnityCG.cginc"
	
				struct appdata
				{
					float4 vertex : POSITION;
					float2 texcoord : TEXCOORD0;
				};
	
				struct v2f
				{
					float4 pos : SV_POSITION;
					float2 uv : TEXCOORD0;
				};
				ENDCG
	
            	CGPROGRAM
				#pragma target 3.0
            	#pragma vertex vert
           		#pragma fragment fdraw
           		 
           		uniform float4x4 _DecalMatrix;
           		uniform sampler2D _MainTex;
				uniform float4 _DecalPos;
				uniform float4 _DecalPos2;
				uniform float4 _MainTex_ST;
				uniform int _DecalAxis;
				uniform int _DecalDeform;
				uniform float _Transparent;

				v2f vert(appdata v)
				{
					v2f o;
					float4 vpos = v.vertex;
					//vpos.x = vpos.x + _DecalPos.x;
					//vpos.y = vpos.y + _DecalPos.y;
					//vpos.z = vpos.z + _DecalPos.z;
					float4 posm = UnityObjectToClipPos(vpos);
					o.pos = posm;
					o.uv = v.texcoord;
					return o;
				}

				float4 fdraw(v2f i) : SV_Target
				{
					float4 c;
					float2 fuv;
					fuv.x = (i.uv.x + _MainTex_ST.z - 0.5) / _MainTex_ST.x + 0.5;
					fuv.y = (i.uv.y + _MainTex_ST.w - 0.5) / _MainTex_ST.y + 0.5;
					c = tex2D(_MainTex, fuv);
					//c.a =  _Transparent;
					c.a = _Transparent >= 0 ? _Transparent : c.a;
					float clampx = clamp(i.uv.x + _MainTex_ST.z, 0.5 - (_MainTex_ST.x*0.5), 0.5 + (_MainTex_ST.x*0.5));
					float clampy = clamp(i.uv.y + _MainTex_ST.w, 0.5 - (_MainTex_ST.y*0.5), 0.5 + (_MainTex_ST.y*0.5));
					if ((i.uv.x + _MainTex_ST.z != clampx) || (i.uv.y + _MainTex_ST.w != clampy))
					{
						c.x = 1.0; c.y = 1.0; c.z = 1.0; c.a = 0.0;
					}
					return c;
				}

            	ENDCG
       		 }
    	}
}
