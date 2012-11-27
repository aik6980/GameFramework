#ifndef NOISESHARED_FXH
#define NOISESHARED_FXH

// Noise Function
// [Q?] find out where is this come from? function=> 6t^5 - 15t^4 + 10t^3 
#define PerlinNoise_Fade(t) ((t) * (t) * (t) * ((t) * ((t) * 6 - 15) + 10))

// Encode/Decode Gradient
float3 PerlinNoise_EncodeGrad( float3 g )
{
	return ConvertSNormToUVCoord(g);
}

float3 PerlinNoise_DecodeGrad( float3 g )
{
	return ConvertUVCoordToSNorm(g);
}

#endif