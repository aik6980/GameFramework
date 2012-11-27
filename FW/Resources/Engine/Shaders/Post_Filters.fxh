#ifndef POST_FILTERS_FXH
#define POST_FILTERS_FXH

#include "Common_PostProcess.fxh"
#include "Post_BlurKernel.inc.fx"

float3 Post_Filters_GaussianBlur( Texture2D ColorTexture, float2 dir, float2 uvInput )
{
	float3 sum = (float3)0;
	int stepCurrent = -(Post_Filters_GaussianBlurStepCount - 1)/2;
	for( int i = 0; i < Post_Filters_GaussianBlurStepCount; ++i, ++stepCurrent )
	{
		float2 uv = uvInput + float2( dir.x*stepCurrent*ViewportSize_OneTexel.x, dir.y*stepCurrent*ViewportSize_OneTexel.y);
		float weight = Post_Filters_GaussianBlurWeight9[i];
		sum += Sample( ColorTexture, g_PointSampler_ClampUV, uv) * weight;
	}
	
	return sum;
} 
 
float4 Post_Filters_GaussianBlurH_PS(OneTexelVertex IN,
			uniform Texture2D ColorTexture) : COLOR
{
	float3 sum = (float3)0;
	sum = Post_Filters_GaussianBlur( ColorTexture, float2(1,0), IN.UV);
	
	return float4( sum, 1.0);
}

float4 Post_Filters_GaussianBlurV_PS(OneTexelVertex IN,
			uniform Texture2D ColorTexture) : COLOR
{
	float3 sum = (float3)0;
	sum = Post_Filters_GaussianBlur( ColorTexture, float2(0,1), IN.UV);
	
	return float4( sum, 1.0);
}


float4 Post_Filters_Bokeh_PS(OneTexelVertex IN,
			uniform Texture2D ColorTexture) : COLOR
{
	float3 sum = (float3)0;
	float weightSum = 0;
	// Bokeh 
	int maxRadius = 4.0; // 4.0 pixels
	for( int x = -maxRadius; x < maxRadius; ++x )
	{
		for( int y = -maxRadius; y < maxRadius; ++y )
		{
			float2 uv = IN.UV + float2(x,y) * ViewportSize_OneTexel;
			
			// pick the longest axis for testing
			// this turns ellipse into circle
			float longestAxis = ViewportSize_OneTexel.x > ViewportSize_OneTexel.y ? ViewportSize_OneTexel.x : ViewportSize_OneTexel.y;
			if( distance( uv, IN.UV ) < maxRadius * longestAxis )
			{
				float3 s = Sample( ColorTexture, g_PointSampler_ClampUV, uv );
				float w = length( s.rgb ) + 0.1; // 0.1? WTF
				weightSum += w;
				sum += s * w;
			}
		}
	}
	
	// average
	sum = sum/weightSum;
	
	return float4( sum, 1.0);
}

#endif