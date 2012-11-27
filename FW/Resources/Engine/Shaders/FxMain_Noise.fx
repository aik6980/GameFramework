#include "Common.fxh"
#include "Common_PostProcess.fxh"
#include "RenderVariables.fxh"

#include "Noise.fxh"

SCRIPT_DEFINITION("Scene", "Postprocess")

float4 MainPS(OneTexelVertex IN) : COLOR
{
	float4 color = (float4)0;
	color.rgb = PerlinNoise( float3(IN.UV * 64, 0)).rrr;
	//color.rgb = ValueNoise( float3(IN.UV * 16, 0)).rrr;
	
	float4 output = float4( color.rgb,1.0);
	return output;
}

float g_ClearDepth <string UIWidget = "none";> = 1.0;

float4 g_ClearColor <
    string UIWidget = "Color";
> = {0,0,0,0};

technique10 Main <
	string Script =
	"ClearSetColor=g_ClearColor;"
	"ClearSetDepth=g_ClearDepth;"
	"Clear=Color0;"
	"Clear=Depth;"
	"ScriptExternal=color;"
	"Pass=Post_Shading;"
	;
> {	
	pass Post_Shading <
	string Script =
	    "Draw=Buffer;"
		;
    > {
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, MainPS() ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthTest_WriteDisable, 0);
		SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}