#ifndef COMMON_FATVERTEX_FXH
#define COMMON_FATVERTEX_FXH

struct FatVertex
{
	float4 Position : POSITION;
	float4 Normal	: NORMAL;
	float4 TexCoord0: TEXCOORD0;
	float4 TexCoord1: TEXCOORD1;
};

#endif