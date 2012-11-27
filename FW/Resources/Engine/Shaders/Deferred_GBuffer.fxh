#ifndef 	DEFERRED_GBUFFER_FXH_
#define		DEFERRED_GBUFFER_FXH_

struct GBufferStruct
{
	float4 Albedo	: SV_TARGET0;
	float4 Normal	: SV_TARGET1;
	float4 Misc		: SV_TARGET2;
};

float2 EncodeNormalXY(float3 iNormal)
{
	float2 encode;
	encode = ConvertSNormToUVCoord(iNormal.xy);
	return encode; 
}

float3 DecodeNormalXY(float2 iNormal)
{
	float3 decode;
	float2 normalizedNormal = ConvertUVCoordToSNorm(iNormal.xy);
	// z^2 = 1 - x^2 - y^2 = 1 - ( x^2 + y^2 )
	float z = sqrt(1 - dot( normalizedNormal.xy, normalizedNormal.xy ));
	decode = float3( normalizedNormal, z );
	
	return decode;
}

float3 DecodeNormal(Texture2D normalTex, float2 uvCoords)
{
	float3 normal = Sample( normalTex, g_PointSampler, uvCoords ).xyz;
	//normal = DecodeNormalXY(normal.xy);
	normal = ConvertUVCoordToSNorm(normal);
	
	return normal;
}

float DecodeDepth(Texture2D depthTex, float2 uvCoords)
{
	float depth = Sample( depthTex, g_PointSampler, uvCoords );
	return depth;
}

float4 PositionFromDepth(Texture2D depthTex, float2 uvCoords, float4x4 matTransformsInv)
{
	float z = DecodeDepth(depthTex, uvCoords);
	
	// get x/w and y/w from the TexCoords
	float x = ConvertUVCoordToSNorm(uvCoords.x);
	float y = ConvertUVCoordToSNorm(1 - uvCoords.y);
	
	float4 posH = float4(x,y,z,1.0);
	
	// transform back to our targetSpace
	float4 posW = mul(posH, matTransformsInv);
	// divided by w to get the position
	return posW/posW.w;
}

float4 PosViewFromDepth(Texture2D depthTex, float2 uvCoords)
{
	return PositionFromDepth(depthTex, uvCoords, ProjectionIXf);
}

GBufferStruct MakeGBuffer(float3 color, float3 normal, float depth )
{
	GBufferStruct output = (GBufferStruct)0;
	output.Albedo.rgb = color.rgb;
	output.Albedo.a = 0; // UNUSED
	//output.Normal.xy = EncodeNormalXY(normal);
	output.Normal.xyz = ConvertSNormToUVCoord(normal);
	
	output.Misc.r = depth;
	
	return output;
}

#endif