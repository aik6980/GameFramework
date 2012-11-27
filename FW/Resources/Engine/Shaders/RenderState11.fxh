#ifndef 	FxcRenderState_FXH_
#define		FxcRenderState_FXH_

// GPU States
RasterizerState DisableCulling
{
    CullMode = NONE;
};

RasterizerState WireFrame_DisableCulling
{
	FillMode = WIREFRAME;
    CullMode = NONE;
};

DepthStencilState DepthTest_WriteEnable
{
	DepthEnable = TRUE;
	DepthWriteMask = ALL;
};

DepthStencilState DepthTestEnable_WriteDisable
{
	DepthEnable = TRUE;
	DepthWriteMask = ALL;
};

DepthStencilState DepthTest_WriteDisable
{
	DepthEnable = FALSE;
	DepthWriteMask = ZERO;
};

BlendState DisableBlend
{
	BlendEnable[0] = FALSE;
};

// Sampler States
SamplerState g_LinearSampler
{
	Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

SamplerState g_PointSampler
{
	Filter = MIN_MAG_MIP_POINT;
    AddressU = Wrap;
    AddressV = Wrap;
};

SamplerState g_PointSampler_ClampUV
{
	Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 Sample( Texture2D t, SamplerState s, float2 uv )
{
	return t.Sample( s, uv); 
}

#endif