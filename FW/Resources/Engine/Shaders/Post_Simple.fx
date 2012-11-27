/*

% Description of my shader.
% Second line of description for my shader.

keywords: material classic

date: YYMMDD

*/

#include "FxcCommon.fxh"

SCRIPT_DEFINITION( "scene", "postprocess" )

RT_COLOR( g_SceneTex, 1.0, 1.0, "A8R8G8B8" )
RT_DEPTH( g_SceneDepth, 1.0, 1.0, "D24S8" )

RT_COLOR( g_SceneHalfRes, 0.25, 0.25, "A8R8G8B8" )

BEGINCB( CbPerObject )
	float4x4 WorldViewProj : WorldViewProjection;
ENDCB

struct VSIn
{
	float4	pos		: POSITION;
	float2	tex0	: TEXCOORD0;
};

// same structure (why typing twice!)
typedef VSIn VSOut;

VSOut mainVS(VSIn inData)
{
	VSOut outData = (VSOut)0;
	//float4 posH = mul(float4(inData.pos.xyz, 1.0), WorldViewProj);
	float4 posH = float4( 0, 0, 0, 1.0 );
	posH.xy = inData.pos.xy;
	posH.z = inData.pos.z;
	outData.pos = posH;
	outData.tex0= inData.tex0;
	return outData;
}

float4 mainPS(VSOut inData) : COLOR 
{
	float4 resultCol = Sample(g_SceneTex, g_LinearSampler, inData.tex0.xy);
	return float4( resultCol.rgb, 1.0 );
}

float4 mainPS1(VSOut inData) : COLOR 
{
	float4 resultCol = Sample(g_SceneHalfRes, g_LinearSampler, inData.tex0.xy);
	return float4( resultCol.rgb, 1.0 );
}

BEGINCB( cbFxcScriptOnly )
	float4 g_ClearColor = float4( 0.0, 0.0, 0.0, 1.0 );
	float g_ClearDepth = 1.0;
ENDCB

technique10 Main10
<
	string Script = 
	"RenderColorTarget0=g_SceneTex;"
	"RenderDepthStencilTarget=;"
	"ClearSetColor=g_ClearColor;"
	"ClearSetDepth=g_ClearDepth;"
	"Clear=Color;"
	"Clear=Depth;"
	"ScriptExternal=color;"
	"Pass=P0;"
	"Pass=P1;";
>
{
	pass P0 <
		string Script =
		"RenderColorTarget0=g_SceneHalfRes;"
		"Draw=Buffer;";
	>
	{
		SetVertexShader( CompileShader( vs_4_0, mainVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mainPS() ) );
	}
	
	pass P1<
		string Script =
		"RenderColorTarget0=;"
		"Draw=Buffer;";
	>
	{
		SetVertexShader( CompileShader( vs_4_0, mainVS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, mainPS1() ) );
	}
}
