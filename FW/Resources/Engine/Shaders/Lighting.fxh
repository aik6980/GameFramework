#ifndef LIGHTING_FXH
#define LIGHTING_FXH

float Lambertian( float3 toLight, float3 surfNorm )
{
	// Half Lambertian
	return ConvertSNormToUVCoord(dot( toLight, surfNorm ));
	//return saturate(dot( toLight, surfNorm ));
}

void LitDirectionalLight_Diffuse( out float3 oCol,
	float3 lightDir,
	float3 surfNorm )
{
	oCol = Lambertian( lightDir, surfNorm );
}

void LitPointLight_Diffuse( out float3 oCol,
	float3 lightPos,
	float3 surfPos,
	float3 surfNorm )
{
	float3 toLight = lightPos - surfPos;
	float3 toLightDir = normalize(toLight);
	
	// distance From Light
	float d = length(toLight);
	// some fixed attenuation
	float3 attnFactor = float3( 1.0, 0.1, 0.01 );
	float attn = 1.0/( attnFactor.x + attnFactor.y*d + attnFactor.z*d*d );
	
	oCol = attn * Lambertian( toLightDir, surfNorm );
}

#endif