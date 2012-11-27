#ifndef RENDERSTATE9_FXH
#define RENDERSTATE9_FXH

// All dummy
sampler g_PointSampler = sampler_state
{
	texture = null;
};

sampler g_PointSampler_ClampUV = sampler_state
{
	texture = null;
};

float4 Sample( Texture2D t, sampler s, float2 uv )
{
	return 0;
}

#endif