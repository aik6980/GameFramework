#ifndef NOISECPU_FXH
#define NOISECPU_FXH

#include "Common.fxh"

// The following code is using FX Composer Magic to run them on CPU
// and evaluate Textures

// [TEST] Noise Texture, Created on CPU
TEXTURE2D_GENERATOR( g_NoiseTestTex, "PerlinNoise_CreateTestTex", "A8B8G8R8", 256, 256);

// permutation table
static int PerlinNoise_PermTbl_Count = 256.0;
static int PerlinNoise_PermTbl[] =
{
	151,160,137,91,90,15,
	131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
	190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
	88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
	77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
	102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
	135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
	5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
	223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
	129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
	251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
	49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
	138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
};

// The reference implementation uses bit-manipulation code to generate the gradient vectors 
// directly from the hash values. 
// Because current pixel shader hardware does not include integer operations, 
// this method is not feasible, so instead we precalculate a small 1D texture containing the 16 gradient vectors. 

// [Q?] I think 12 points should be enough?
// [A] 12 points enough from Perlin'paper, but 16 points will align with 2^x texture and more random
static float3 PerlinNoise_Gradients[] = 
{
	float3( 1, 1, 0),
	float3(-1, 1, 0),
	float3( 1,-1, 0),
	float3(-1,-1, 0),
	float3( 1, 0, 1),
	float3(-1, 0, 1),
	float3( 1, 0,-1),
	float3(-1, 0,-1),
	float3( 0, 1, 1),
	float3( 0,-1, 1),
	float3( 0, 1,-1),
	float3( 0,-1,-1),
	float3( 1, 1, 0),
	float3( 0,-1, 1),
	float3(-1, 1, 0),
	float3( 0,-1,-1)
};

float4 PerlinNoise_CreatePermTex( float Position : POSITION ) : COLOR
{
	float p = PerlinNoise_PermTbl[ Position * 255.0 ] / (255.0);
	return float4(p.rrr, 1.0);
}

float4 PerlinNoise_CreateGradTex( float Position : POSITION ) : COLOR
{
	float3 g = PerlinNoise_Gradients[ Position * 16 ];
	return float4(PerlinNoise_EncodeGrad(g), 1.0);
}

int PermLookupWrap(int i)
{
	return PerlinNoise_PermTbl[i % 256];
}

float3 GradLookupWrap(int i, float3 p)
{
	float3 g = PerlinNoise_Gradients[i % 16];
	return dot( g, p);
}

// Create Pre-Calculated Perm2D Texture
float4 PerlinNoise_CreatePerm2dTex( float2 Position : POSITION ) : COLOR
{
	Position *= 256.0;
	int A 	= PermLookupWrap(Position.x) + Position.y;
	int AA	= PermLookupWrap(A);
	int AB  = PermLookupWrap(A + 1);
	
	int B 	= PermLookupWrap(Position.x + 1) + Position.y;
	int BA	= PermLookupWrap(B);
	int BB  = PermLookupWrap(B + 1);
	
	return float4( AA, AB, BA, BB) / 255.0;
}

float4 PerlinNoise_CreateGradPermTex( float Position : POSITION ) : COLOR
{
	float3 g = PerlinNoise_Gradients[ PerlinNoise_PermTbl[ Position * 256.0 ] % 16 ];
	return float4(PerlinNoise_EncodeGrad(g), 1.0);
}

float4 PerlinNoise_CreateTestTex( float2 Position : POSITION ) : COLOR
{
	float3 p = float3(Position.xy * 64, 0);
	
	float3 posInt = fmod(floor(p), 256.0);
	float3 posFrac = frac(p);
	// posFrag => 0..1
	float3 posInterp = PerlinNoise_Fade(posFrac);
	
	// hash Coords for 6, out of 8 corners
	// [Q?] Find out more about this!
	float A  = PermLookupWrap( posInt.x ) + posInt.y;
	float AA = PermLookupWrap( A ) + posInt.z;
	float AB = PermLookupWrap( A + 1 ) + posInt.z;
	
	float B  = PermLookupWrap( posInt.x + 1 ) + posInt.y;
	float BA = PermLookupWrap( B ) + posInt.z;
	float BB = PermLookupWrap( B + 1) + posInt.z;
	
	// Interpolate the fraction and blend from 8 corners
	float x0 = lerp(GradLookupWrap(PermLookupWrap(AA), posFrac), GradLookupWrap(PermLookupWrap(BA), posFrac + float3(-1, 0, 0)), posInterp.x);
	float x1 = lerp(GradLookupWrap(PermLookupWrap(AB), posFrac + float3(0, -1, 0)), GradLookupWrap(PermLookupWrap(BB), posFrac + float3(-1, -1, 0)), posInterp.x);
	float x2 = lerp(GradLookupWrap(PermLookupWrap(AA + 1), posFrac + float3(0, 0, -1)), GradLookupWrap(PermLookupWrap(BA + 1), posFrac + float3(-1, 0, -1)), posInterp.x);
	float x3 = lerp(GradLookupWrap(PermLookupWrap(AB + 1), posFrac + float3(0, -1, -1)), GradLookupWrap(PermLookupWrap(BB + 1), posFrac + float3(-1, -1, -1)), posInterp.x);
	
	float y0 = lerp( x0, x1, posInterp.y);
	float y1 = lerp( x2, x3, posInterp.y);
	
	float z = lerp( y0, y1, posInterp.z );
	
	return z;
}

// END OF CPU CODE

#endif