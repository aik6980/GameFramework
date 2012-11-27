#ifndef DEFERRED_SSAO_FXH
#define DEFERRED_SSAO_FXH

#include "Common.fxh"

// some adjustable Constants
static float Deferred_SSAO_Param_Offset = 0.02;
static float Deferred_SSAO_Param_Radius = 0.03;
static float Deferred_SSAO_Param_Strength = 0.0005;
static float Deferred_SSAO_Param_Falloff = 0.0;

// random number generator // WTF?
static float Deferred_SSAO_RandF(float2 uv)
{
	// note: 
	// - whatever this is, it returns 0..1
	float toBigNumber = dot(uv.xy , float2(12.9898,78.233));
	float addRandomness = sin(toBigNumber) * 43758.5453;
	float moreRandomness = frac(addRandomness);
    return moreRandomness;
}

// Noise Texture
TEXTURE2D( Deferred_SSAO_NoiseTex, "Engine/Textures/Noise.dds");

static int Deferred_SSAO_NumSamples = 10;
// these are the random vectors inside a unit sphere
// [Q?] we should be able to just regenerate these data?
static float3 Deferred_SSAO_RandUnitVectors[10] =
{
	float3(0.13790712, 0.24864247, 0.44301823),
    float3(0.33715037, 0.56794053, -0.005789503),
    float3(0.06896307, -0.15983082, -0.85477847),
    float3(-0.014653638, 0.14027752, 0.0762037),
    float3(0.010019933, -0.1924225, -0.034443386),
    float3(-0.35775623, -0.5301969, -0.43581226),
    float3(-0.3169221, 0.106360726, 0.015860917),
    float3(0.010350345, -0.58698344, 0.0046293875),
	float3(-0.053382345, 0.059675813, -0.5411899),
    float3(0.035267662, -0.063188605, 0.54602677)

};

float4 Deferred_SSAOPS(OneTexelVertex IN,
			uniform Texture2D NormalTex,
			uniform Texture2D DepthTex) : COLOR
{
	// data in view space
	float3 normalView = DecodeNormal(NormalTex,IN.UV);
	float4 posView = PosViewFromDepth(DepthTex, IN.UV);
	
	//grab a normal for reflecting the sample rays later on
	float2 randUV = Deferred_SSAO_Param_Offset * Deferred_SSAO_RandF(IN.UV) * IN.UV; // WTF?
	float3 randNormal = Sample(Deferred_SSAO_NoiseTex, g_PointSampler, randUV);
    randNormal = ConvertUVCoordToSNorm(randNormal);
	
	float acc = 0.0; 
	for(int i=0; i< Deferred_SSAO_NumSamples; ++i )
	{
		// get a vector (randomized inside of a sphere with radius 1.0) from a texture and reflect it
		
		// Note1: reflect(Deferred_SSAO_RandUnitVectors[i], randNormal) 
		// => this effectively with rotate our fixed set of unit vectors (sphere) to create random samples
		// => do it this way so we don't have to sample noise texture SAMPLES times (save some TextureUnit work)
		// Deferred_SSAO_Param_Radius
		// => radius of the area we care about
    	float3 ray = Deferred_SSAO_Param_Radius * reflect(Deferred_SSAO_RandUnitVectors[i], randNormal);
		
		// get the depth of the occluder fragment
		// Note: sign(dot(ray,normalView) )* ray.xy
		// => effectively will flip the random vertor so it turns Sphere into HemiSphere, regarding to the Normal direction
		// screenSpacePosToTest => screenSpace Pixel that we want to calculate Occulding Factor
    	float2 screenSpacePosToTest = IN.UV + sign(dot(ray,normalView) )* ray.xy;
		
		float3 occluder_normalView = DecodeNormal(NormalTex, screenSpacePosToTest);
		float4 occluder_posView = PosViewFromDepth(DepthTex, screenSpacePosToTest);
		
		// KEY: see how much Occlusion from Normal
		float occludeFactorFromNorm = (1.0-dot(occluder_normalView,normalView));
		
		// KEY: see how much Occlusion from Depth
    	// if depthDifference is negative = occluder is behind current fragment
		float zRange = 1000.0; // estimated ZNear - ZFar;
    	float depthDifference = (posView.z-occluder_posView.z)/zRange;
		// the falloff equation, starts at falloff and is kind of 1/x^2 falling
		float fadingSpeed = (1.0-smoothstep(Deferred_SSAO_Param_Falloff,Deferred_SSAO_Param_Strength,depthDifference));
		float occludeFactorFromDepth = step(Deferred_SSAO_Param_Falloff,depthDifference) * fadingSpeed;
		
		// Total occluding factor
		//acc += occludeFactorFromDepth;
		acc += occludeFactorFromNorm * occludeFactorFromDepth;
	}
	
	// average the result then save to the texture
	float avg = acc/(float)Deferred_SSAO_NumSamples;
	
	// [Q?] it seems like we had calculate How much the pos doesn't been occluded
	// so we have to flip it here?
	avg = 1.0 - avg;
	return float4(avg.rrr, 1.0);
}

#endif