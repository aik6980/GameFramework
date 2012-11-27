#include "Common.fxh"
#include "Common_FatVertex.fxh"
#include "RenderState11.fxh"
#include "RenderVariables.fxh"

BEGINCB( cbTess_Generic )
	float g_TessFactor = 1.0;
ENDCB

// Struct
struct VSOut
{
	float4 Position : POSITION;
};


// Output from Hull Shader
struct HSOutConst
{
	float TessFactor[3] : SV_TessFactor;
	float InsideTessFactor : SV_InsideTessFactor;
};



// Functions

// Pass-Through VS
FatVertex Tess_GenericVS(FatVertex IN)
{
	FatVertex OUT = (FatVertex)0;
	OUT.Position = float4(IN.Position.xyz * 0.5f, 1.0);
	
	return OUT;
}

// Hull Shader
HSOutConst Tess_GenericHSConst( InputPatch<FatVertex, 3> IN )
{
	HSOutConst OUT = (HSOutConst)0;
	
	OUT.TessFactor[0] = OUT.TessFactor[1] = OUT.TessFactor[2] = g_TessFactor;
	OUT.InsideTessFactor = g_TessFactor;
	
	return OUT;
}

[domain("tri")]
// [Q?] fraction_even! fraction_odd find out more more these different scheme
[partitioning("fractional_even")] 
[outputtopology("triangle_cw")]
[patchconstantfunc("Tess_GenericHSConst")]
[outputcontrolpoints(3)]
// Note: CPID => Constant Patch ID?
FatVertex Tess_GenericHS( InputPatch<FatVertex, 3> IN,
	uint uCPID : SV_OutputControlPointID )
{
	FatVertex OUT = (FatVertex)0;;
	OUT.Position = IN[uCPID].Position;
	
	return OUT;
}

// Domain Shader
VSOut Tess_GenericDS( HSOutConst HSConstData, OutputPatch<FatVertex, 3> IN,
	float3 barycentricCoords : SV_DomainLocation )
{
	VSOut OUT;
	
	// first interpolation to get the vertex position
	float3 posObj = barycentricCoords.x * IN[0].Position.xyz +
		barycentricCoords.y * IN[1].Position.xyz +
		barycentricCoords.z *IN[2].Position.xyz; 
	
	// here we do displacement map
	
	// output like normally we do to VertexShader
	float4x4 objToWorld, objToView, objToProj;
	OUT.Position = Transforms( objToProj, objToView, objToWorld, posObj.xyz);

	OUT.Position = float4( posObj.xyz, 1.0);
	
	return OUT;
}

float4 Tess_GenericPS( VSOut IN ) : SV_TARGET0 
{
	return float4( 1, 0, 1, 1);
} 

// Techniques
technique10 Main <
	string Script = ""
	;
> {
    pass GameScene <
	string Script = "";
    > {
        SetVertexShader( CompileShader( vs_5_0, Tess_GenericVS() ) );
		SetHullShader( CompileShader( hs_5_0, Tess_GenericHS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_5_0, Tess_GenericPS() ) );
                
        SetRasterizerState(WireFrame_DisableCulling);       
		SetDepthStencilState(DepthTest_WriteEnable, 0);
		SetBlendState(DisableBlend, float4( 0.0f, 0.0f, 0.0f, 0.0f ), 0xFFFFFFFF);
    }
}
