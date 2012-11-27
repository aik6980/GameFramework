#include "Common.fxh"
#include "Common_FatVertex.fxh"
#include "RenderVariables.fxh"
#include "Deferred_GBuffer.fxh"
#include "Noise.fxh"

struct VSOut
{
	float4 Position		: POSITION;
	float4 Normal		: NORMAL;
	float4 TexCoord		: TEXCOORD0;
	float4 PosView		: TEXCOORD1;
	float4 NormView		: TEXCOORD2;
	float4 PosH			: TEXCOORD3;
};

VSOut Deferred_GenericVS(FatVertex input)
{
	VSOut output = (VSOut)0;
	
	float4x4 objToWorld, objToView, objToProj;
	output.Position = Transforms( objToProj, objToView, objToWorld, input.Position.xyz);
	output.PosH = output.Position;
	
	output.Normal.xyz = mul( input.Normal.xyz, (float3x3)objToWorld);
	
	output.PosView = mul( float4(input.Position.xyz, 1.0), objToView);
	output.NormView.xyz = mul( input.Normal.xyz, (float3x3)objToView);
	
	output.TexCoord.xy = input.TexCoord0.xy;
	
	return output;
}

GBufferStruct Deferred_GenericPS(VSOut input)
{
	float3 normal = normalize(input.NormView.xyz);
	float3 color = g_MainLightCol * saturate(dot( -g_MainLightDir, input.Normal )) ;
	
	float3 lightPosView = mul( float4(g_PointLight0Pos, 1.0), ViewXf); 
	float3 toLightView = normalize(lightPosView - input.PosView.xyz);
	color = saturate(dot( toLightView, normal ));
	
	color = float3(1,1,1);
	
	float depth = input.PosH.z/input.PosH.w;
	
	GBufferStruct output;
	output = MakeGBuffer( color, normal, depth );
	
	return output;
}

// FXC Specific
SCRIPT_DEFINITION("Object", "Standard") 

float4 gClearColor <
    string UIWidget = "Color";
    string UIName = "Background";
> = {0,0,0,0};

float gClearDepth <string UIWidget = "none";> = 1.0;

technique10 Main <
	string Script = 
	"Pass=GameScene;"
	;
> {
    pass GameScene <
	string Script = 
		"Draw=Geometry;"
		;
    > {
        SetVertexShader( CompileShader( vs_4_0, Deferred_GenericVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, Deferred_GenericPS() ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthTest_WriteEnable, 0);
		SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}

technique Main9
{
	pass p0
	{
		// Dummy, no directX9 impl
    }
}