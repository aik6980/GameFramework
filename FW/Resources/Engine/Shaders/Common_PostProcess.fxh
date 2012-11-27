#ifndef COMMON_POSTPROCESS_FXH
#define COMMON_POSTPROCESS_FXH

float2 ViewportSize : VIEWPORTPIXELSIZE <
    //string UIWidget="None";
>;

static float2 ViewportSize_OneTexel = (float2(1.0,1.0)/ViewportSize);
static float2 ViewportSize_HalfTexel = (float2(0.5,0.5)/ViewportSize);

// i don't think we need the offset for D3D10 API
//static float2 ViewportOffset = ViewportSize_HalfTexel;
static float2 ViewportOffset = (float2)0;

// Shared VertexShader
struct OneTexelVertex {
    float4 Position	: POSITION;
    float2 UV		: TEXCOORD0;
};

OneTexelVertex ScreenQuadVS(
		float3 Position : POSITION, 
		float2 UV	: TEXCOORD0 ) 
{
    OneTexelVertex OUT = (OneTexelVertex)0;
    OUT.Position = float4(Position, 1);
    OUT.UV = float2(UV.xy + ViewportOffset);
    return OUT;
}

#endif