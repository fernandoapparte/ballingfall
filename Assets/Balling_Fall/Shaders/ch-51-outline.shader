Shader "Holistic P2/ch-51-outline" {
/*
Outline has 2 ways of making it:
1-The logical one, which you'll understand wihtout problems: 
We use 2 objects, 1 is our object and other is the same object which is bigger using vertex extruding.
The second object we make a single color and we turn the buffer off and thats is a outline , inefficient yes, but it works.

The second technique is a outline in 3D, not even the parrot would understand it, but it also works but differently because is 3d.
*/
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
		_OutlineColor ("Outline Color", Color) =(0,0,0,1)
		_Outline("Outline Width", Range (.002, 0.1)) = .005
	}
	SubShader {

		//This is the inflated  version of the object
		//To make the outline result stays behing the object, we need to do two little things:
		//1-ZBuffer needs to be off (and on for drawing the object over )
		//2-Render Queue needs to be Transpartent in order to see the results in scene.

			ZWrite off 
			Tags { 	"Queue" ="Transparent"	} 
			CGPROGRAM
			#pragma surface surf Lambert vertex:vert
			struct Input {
				float2 uv_MainTex;
			};
			float _Outline;
			float4 _OutlineColor;

			void vert(inout appdata_full v) {
				v.vertex.xyz += v.normal * _Outline;
			}

			sampler2D _MainTex;
			void surf (Input IN, inout SurfaceOutput o) {
				o.Emission = _OutlineColor.rgb;
			}
		ENDCG

		ZWrite on
		CGPROGRAM
			#pragma surface surf Lambert
			struct Input {
				float2 uv_MainTex;
			};
			sampler2D _MainTex;
			void surf (Input IN, inout SurfaceOutput o) {
				o.Albedo= tex2D (_MainTex,IN.uv_MainTex).rgb;
			}
		ENDCG
	}
	FallBack "Diffuse"
}
