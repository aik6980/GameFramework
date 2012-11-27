#ifndef 	RENDERVARIABLES_FXH_
#define		RENDERVARIABLES_FXH_

#include "Common.fxh"
#include "Scene_Lights.fxh"

BEGINCB( cbPerFrame )
	float4x4 ViewXf : View < string UIWidget="None"; >;
	float4x4 ProjectionXf : Projection;// < string UIWidget="None"; >;
	float4x4 ViewIXf : ViewInverse < string UIWidget="None"; >;
	float4x4 ProjectionIXf : ProjectionInverse ;//< string UIWidget="None"; >;
	float4x4 ViewProjectionIXf : ViewProjectionInverse ;//< string UIWidget="None"; >;
	
ENDCB

BEGINCB( cbPerObject )
	float4x4 WorldXf : World < string UIWidget="None"; >;
	float4x4 WorldIXf : WorldInverse < string UIWidget="None"; >;
	float4x4 WorldITXf : WorldInverseTranspose < string UIWidget="None"; >;
ENDCB

float4 Transforms(out float4x4 objToProj, out float4x4 objToView, out float4x4 objToWorld, float3 posObj)
{
	objToWorld = WorldXf;
	objToView = mul(WorldXf, ViewXf);
	objToProj = mul(objToView, ProjectionXf);
	
	return mul( float4( posObj, 1.0), objToProj ); 
}

float GetZNear()
{
	float4 minZ = float4(0.0,0.0,0.0,1.0);
	return mul( minZ, ProjectionIXf ).z;
}

#endif