#include "Common.fxh"
#include "Common_PostProcess.fxh"
#include "RenderVariables.fxh"
#include "Deferred_GBuffer.fxh"

#include "Lighting.fxh"
#include "Noise.fxh"

#include "Post_Filters.fxh"
#include "Deferred_SSAO.fxh"

SCRIPT_DEFINITION("Scene", "Postprocess") 

// Very Slow Big Format, oh well, this is the easiest one 
// to debug and learn the technique!
RT_COLOR( g_GameMrt0, 1.0, 1.0, "A32B32G32R32F" )
RT_COLOR( g_GameMrt1, 1.0, 1.0, "A32B32G32R32F" )
RT_COLOR( g_GameMrt2, 1.0, 1.0, "A32B32G32R32F" )
RT_DEPTH( g_GameDepth, 1.0, 1.0, "D24S8" )

// SSAO
RT_COLOR( g_SSAOTex, 1.0, 1.0, "L8" )

// Gaussian Blur
RT_COLOR( g_GaussianBlur0, 1.0, 1.0, "A32B32G32R32F" )
RT_COLOR( g_GaussianBlur1, 1.0, 1.0, "A32B32G32R32F" )

// Bokeh
RT_COLOR( g_Bokeh, 1.0, 1.0, "A32B32G32R32F" )

float4 MainPS(OneTexelVertex IN) : COLOR
{
	float4 color = Sample( g_GameMrt0, g_PointSampler, IN.UV );
	
	//color.rgb = PerlinNoise( float3(IN.UV * 64, 0)).rrr;
	//color.rgb = ValueNoise( float3(IN.UV * 16, 0)).rrr;
	
	//float3 g = Sample( g_GradTexture, g_PointSampler, (IN.UV.y * 128) / 16.0 );
	//g = PerlinNoise_DecodeGrad(g);
	//float3 g1 = PerlinNoise_Gradients[ fmod( floor(IN.UV.y * 128), 16) ];
	//color.rgb = float3( 1, 1, 1) - (g - g1);
	
	float3 normalView = DecodeNormal(g_GameMrt1,IN.UV);
	float4 posView = PosViewFromDepth(g_GameMrt2,IN.UV);
	
	float3 lightPosView = mul( float4(g_PointLight0Pos,1.0), ViewXf); 
	
	float ao = Sample( g_SSAOTex, g_PointSampler, IN.UV ).r;
	ao = Sample( g_GaussianBlur1, g_PointSampler, IN.UV ).r;
	//ao = 1.0;
	
	float3 diffuse = color.rgb;
	LitPointLight_Diffuse( diffuse, lightPosView, posView, normalView );
	diffuse = ao * diffuse * color.rgb;
	
	float4 output = float4( diffuse.rgb,1.0);
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
	"RenderColorTarget1=g_GameMrt1;"
	"RenderColorTarget2=g_GameMrt2;"
	"RenderDepthStencilTarget=g_GameDepth;"
	"ClearSetColor=g_ClearColor;"
	"ClearSetDepth=g_ClearDepth;"
	"Clear=Color0;"
	"ClearSetColor=g_ClearNormal;"
	"Clear=Color1;"
	"ClearSetColor=g_ClearColor;"
	"Clear=Color2;"
	"Clear=Depth;"
	"ScriptExternal=color;"
	"Pass=Post_SSAO;"
	"Pass=Post_GaussianBlurH;"
	"Pass=Post_GaussianBlurV;"
	"Pass=Post_DeferredShading;"
	;
> {
	pass Post_SSAO <
	string Script =
		"RenderColorTarget0=g_SSAOTex;"
		"RenderColorTarget1=;"
		"RenderColorTarget2=;"
	    "RenderDepthStencilTarget=;"
		"ClearSetColor=g_ClearColor;"
		"Clear=Color0;"
	    "Draw=Buffer;"
		;
    > {
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, Deferred_SSAOPS(g_GameMrt1, g_GameMrt2) ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthTest_WriteDisable, 0);
		SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
	
	pass Post_GaussianBlurH <
	string Script =
		"RenderColorTarget0=g_GaussianBlur0;"
		"RenderColorTarget1=;"
		"RenderColorTarget2=;"
	    "RenderDepthStencilTarget=;"
		"ClearSetColor=g_ClearColor;"
		"Clear=Color0;"
	    "Draw=Buffer;"
		;
    > {
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, Post_Filters_GaussianBlurH_PS(g_SSAOTex) ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthTest_WriteDisable, 0);
		SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
	
	pass Post_GaussianBlurV <
	string Script =
		"RenderColorTarget0=g_GaussianBlur1;"
		"RenderColorTarget1=;"
		"RenderColorTarget2=;"
	    "RenderDepthStencilTarget=;"
		"ClearSetColor=g_ClearColor;"
		"Clear=Color0;"
	    "Draw=Buffer;"
		;
    > {
        SetVertexShader( CompileShader( vs_4_0, ScreenQuadVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, Post_Filters_GaussianBlurV_PS(g_GaussianBlur0) ) );
                
        SetRasterizerState(DisableCulling);       
		SetDepthStencilState(DepthTest_WriteDisable, 0);
		SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
	
	pass Post_DeferredShading <
	string Script =
		"RenderColorTarget0=;"
		"RenderColorTarget1=;"
		"RenderColorTarget2=;"
	    "RenderDepthStencilTarget=;"
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