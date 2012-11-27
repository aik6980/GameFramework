#include "Common.fxh"
#include "Common_FatVertex.fxh"
#include "RenderState11.fxh"
#include "RenderVariables.fxh"

#include "FxRendercraft_Sh.fxh"

// TODO: add effect parameters here.
float3 LightCol = float3(0.75, 0.75, 0.75);
float3 LightDir = float3(-1.0, -1.0, -1.0);

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
	float3 Normal	: TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(FatVertex IN)
{
    VertexShaderOutput OUT;
	
	// output like normally we do to VertexShader
	float4x4 objToWorld, objToView, objToProj;
	OUT.Position = Transforms( objToProj, objToView, objToWorld, IN.Position.xyz);

    // TODO: add your vertex shader code here.
	float3 worldNormal = mul(IN.Normal.xyz, (float3x3)objToWorld);
	OUT.Normal = worldNormal.xyz;

    return OUT;
}

float4 PixelShaderFunction(VertexShaderOutput IN) : SV_TARGET
{
    // TODO: add your pixel shader code here.
	float4 color = float4( 0.5, 0.0, 0.5, 1.0);

	// lighting
	float3 toLight = -normalize(LightDir);
	float3 normalW = normalize(IN.Normal);
	//float3 diffuse = LightCol * clamp(dot(normalW,toLight), 0.0, 1.0);
	float3 diffuse = Sh_light(normalW, groove); 

	float3 ambient = 0.1;

	color.rgb = diffuse + ambient;

    return color;
}

technique11 Technique1
{
    pass Pass1
    {
        SetVertexShader( CompileShader( vs_5_0, VertexShaderFunction() ) );
        SetPixelShader( CompileShader( ps_5_0, PixelShaderFunction() ) );
    }
}
