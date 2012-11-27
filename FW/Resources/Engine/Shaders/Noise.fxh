#ifndef NOISE_FXH
#define NOISE_FXH

#include "Common.fxh"
#include "NoiseShared.fxh"

// Stupid FXComposer!
// TextureShader is not working with DX11!
//#define GEN_PROCEDURAL_TEXTURE
#ifndef GEN_PROCEDURAL_TEXTURE
TEXTURE2D( g_PermTexture, "Engine/Textures/Gen_PermTexture.dds" );
TEXTURE2D( g_GradTexture, "Engine/Textures/Gen_GradTexture.dds" );

//TEXTURE2D( g_Perm2dTex, "Engine/Textures/Gen_Perm2dTex.dds" );
//TEXTURE2D( g_GradPermTex, "Engine/Textures/Gen_GradPermTex.dds" );

#else
// Code that run on the CPU Side
#include "NoiseCPU.fxh"

TEXTURE2D_GENERATOR( g_PermTexture, "PerlinNoise_CreatePermTex", "L8", 256, 1);
TEXTURE2D_GENERATOR( g_GradTexture, "PerlinNoise_CreateGradTex", "A8B8G8R8", 16, 1);

TEXTURE2D_GENERATOR( g_Perm2dTex, "PerlinNoise_CreatePerm2dTex", "A8B8G8R8", 256, 256);
TEXTURE2D_GENERATOR( g_GradPermTex, "PerlinNoise_CreateGradPermTex", "A8B8G8R8", 256, 1);

#endif

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

float perm(float x)
{
  //return Sample( g_PermTexture, g_PointSampler, x / 256.0) * 256.0;
  return PerlinNoise_PermTbl[ fmod( floor(x), 256 ) ];
}

float ValueNoise(float3 p)
{
	float3 posInt = fmod(floor(p), 256.0);
	float3 posFrac = frac(p);
	// Perm2D function
	float A  = perm(perm( posInt.x ) + posInt.y) / 256.0;
	A = ConvertUVCoordToSNorm(A);
	// posFrag => 0..1
	float3 posInterp = PerlinNoise_Fade(posFrac);
	
	// Build weighs for 2x2 interpolation grid
	float4 interp0 = float4( 1 - posInterp.x, posInterp.x, 1 - posInterp.x, posInterp.x);
	float4 interp1 = float4( 1 - posInterp.y, 1 - posInterp.y, posInterp.y, posInterp.y);
	float blendWeigh = interp0 * interp1;
	
	return dot( float4(posInterp, 1.0), blendWeigh); 
}

#if 1

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

float grad(float x, float3 p)
{
	float3 g = Sample( g_GradTexture, g_PointSampler, x / 16.0 );
	g = PerlinNoise_DecodeGrad(g);
	g = PerlinNoise_Gradients[ fmod( floor(x), 16) ];
	return dot(g, p);
}

float PerlinNoise(float3 p)
{	
	float3 posInt = fmod( floor(p), 256.0 );
	float3 posFrac = frac(p);
	
	// posFrag => 0..1
	float3 posInterp = PerlinNoise_Fade(posFrac);
	
	// hash Coords for 6, out of 8 corners
	// [Q?] Find out more about this!
	float A  = perm( posInt.x ) + posInt.y;
	float AA = perm( A ) + posInt.z;
	float AB = perm( A + 1 ) + posInt.z;
	
	float B  = perm( posInt.x + 1 ) + posInt.y;
	float BA = perm( B ) + posInt.z;
	float BB = perm( B + 1) + posInt.z; 
	
	// Interpolate the fraction and blend from 8 corners
	float x0 = lerp(grad(perm(AA), posFrac), grad(perm(BA), posFrac + float3(-1, 0, 0)), posInterp.x);
	float x1 = lerp(grad(perm(AB), posFrac + float3(0, -1, 0)), grad(perm(BB), posFrac + float3(-1, -1, 0)), posInterp.x);
	
	float x2 = lerp(grad(perm(AA + 1), posFrac + float3(0, 0, -1)), grad(perm(BA + 1), posFrac + float3(-1, 0, -1)), posInterp.x);
	float x3 = lerp(grad(perm(AB + 1), posFrac + float3(0, -1, -1)), grad(perm(BB + 1), posFrac + float3(-1, -1, -1)), posInterp.x);
	
	float y0 = lerp( x0, x1, posInterp.y);
	float y1 = lerp( x2, x3, posInterp.y);
	
	float z = lerp( y0, y1, posInterp.z );
	
	return z;
}
#else

// optimized version
float4 perm2d(float2 p)
{
	return Sample( g_Perm2dTex, g_PointSampler, p );
}

float grad(float x, float3 p)
{	
	float3 g = Sample( g_GradPermTex, g_PointSampler, x);
	g = PerlinNoise_DecodeGrad(g);
	return dot(g, p);
}

float PerlinNoise(float3 p)
{
	float3 P = fmod(floor(p), 256.0);	// FIND UNIT CUBE THAT CONTAINS POINT
  	p -= floor(p);						// FIND RELATIVE X,Y,Z OF POINT IN CUBE.
	float3 f = PerlinNoise_Fade(p);     // COMPUTE FADE CURVES FOR EACH OF X,Y,Z.

	P = P / 256.0;
	const float one = 1.0 / 256.0;
	
    // HASH COORDINATES OF THE 8 CUBE CORNERS
	float4 AA = perm2d(P.xy) + P.z;
 
	// AND ADD BLENDED RESULTS FROM 8 CORNERS OF CUBE
  	return lerp( lerp( lerp( grad(AA.x, p ),  
                             grad(AA.z, p + float3(-1, 0, 0) ), f.x),
                       lerp( grad(AA.y, p + float3(0, -1, 0) ),
                             grad(AA.w, p + float3(-1, -1, 0) ), f.x), f.y),
                             
                 lerp( lerp( grad(AA.x+one, p + float3(0, 0, -1) ),
                             grad(AA.z+one, p + float3(-1, 0, -1) ), f.x),
                       lerp( grad(AA.y+one, p + float3(0, -1, -1) ),
                             grad(AA.w+one, p + float3(-1, -1, -1) ), f.x), f.y), f.z);
}

#endif

// Skew and unskew factors are a bit hairy for 2D, so define them as constants
// F2 => This is (sqrt(3.0)-1.0)/2.0
// G2 => This is (3.0-sqrt(3.0))/6.0
static const float F2 = 0.366025403784;
static const float G2 = 0.211324865405;
float SimplexNoise(float2 p)
{
	// Skew the (x,y) space to determine which cell of simplices we're in
	float 	u = ( p.x + p.y ) * F2;
	float2  Pi= float2( p + u ); // Pi => position in Simplex Space.
	float 	v = ( Pi.x + Pi.y ) * G2;
	float2  P0= Pi - v; // unskew the cell's origin back to (x,y) space
}

#endif