#include "Common.fxh"
#include "Common_PostProcess.fxh"

#include "Post_Filters.fxh"

SCRIPT_DEFINITION("Scene", "Postprocess") 

RT_COLOR( g_GameMrt0, 1.0, 1.0, "A8B8G8R8" )
RT_DEPTH( g_GameDepth, 1.0, 1.0, "D24S8" )

TEXTURE2D( g_ResolveFromGame, "Engine/Textures/Environment.jpg" );

float4 GameScenePS(OneTexelVertex IN) : COLOR
{
	float4 color = Sample( g_ResolveFromGame, g_PointSampler, IN.UV );
	
	float4 output = float4( color.rgb,1.0);
	return output;
}

float4 Post_FilterPS(OneTexelVertex IN) : COLOR
{
	float4 color = Sample( g_GameMrt0, g_PointSampler, IN.UV );
	
	float4 output = float4( color.rgb,1.0);
	return output;
}

float4 g_ClearColor <
    string UIWidget = "Color";
> = {0,0,0,0};

float4 g_ClearNormal <
    string UIWidget = "Color";
> = {0.5,0.5,0.5,0};

float g_ClearDepth <string UIWidget = "none";> = 1.0;

technique10 Main <
	string Script =
	"RenderColorTarget0=g_GameMrt0;"
	"RenderDepthStencilTarget=g_GameDepth;"
	"ClearSetColor=g_ClearColor;"
	"ClearSetDepth=g_ClearDepth;"
	"Clear=Color0;"
	"Clear=Depth;"
	"ScriptExternal=color;"
	"Pass=GameScene;"
	"Pass=Post_Filter;"
	;
> {
	pass GameScene <
	string Script =
	    "Draw=Buffer;"
		;
    > {
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, GameScenePS() ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthTest_WriteDisable, 0);
		SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
	
	pass Post_Filter <
	string Script =
		"RenderColorTarget0=;"
	    "RenderDepthStencilTarget=;"
	    "Draw=Buffer;"
		;
    > {
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, Post_Filters_Bokeh_PS(g_GameMrt0) ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthTest_WriteDisable, 0);
		SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}