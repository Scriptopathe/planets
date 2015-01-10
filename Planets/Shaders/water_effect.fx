#include "Shaders\\core.fx"
#include "Shaders\\scattering.fx"
/* -----------------------------------------------------------------
* Varibales / buffers.
* ---------------------------------------------------------------*/
// Buffer mis à jour toutes les frames.

texture2D xTexture;
texture2D xTexture2;
texture2D xTexture3;
//texture2D xTexture4;


SamplerState s
{
	Filter = ANISOTROPIC;//MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};


struct VertexIn
{
	float4 PosL    : POSITION;
	float4 NormalL : NORMAL;
	float2 TexCoord  : TEXTURE;
	float4 NormalP : NORMAL;
	float TexId : TEXTURE_ID;
};

struct VertexOut
{
	float4 PosH    : SV_POSITION;
	float3 PosW    : POSITION;
	float3 NormalW : NORMAL;
	float2 TexCoord : TEXTURE;
	float TexId : TEXTURE_ID;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;

	vout.PosW = mul(vin.PosL, xWorld).xyz;
	vout.NormalW = vin.NormalL.xyz;//mul(vin.NormalL, (float3x3)xWorldInvTranspose);

	vout.PosH = mul(vin.PosL, xWorldViewProj);
	vout.TexCoord = vin.TexCoord;
	vout.TexId = vin.TexId;
	return vout;
}

float4 PS(VertexOut pin) : SV_Target
{
	pin.NormalW = normalize(pin.NormalW);
	float3 toEyeW = normalize(xEyePosW - pin.PosW);

	float4 ambient = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 diffuse = float4(0.0f, 0.0f, 0.0f, 0.0f);
	float4 spec = float4(0.0f, 0.0f, 0.0f, 0.0f);

	float4 A, D, S;

	ComputeDirectionalLight(xMaterial, xDirLight, pin.NormalW, toEyeW, A, D, S);
	ambient += A;
	diffuse += D;
	spec += S;

	ComputePointLight(xMaterial, xPointLight, pin.PosW, pin.NormalW, toEyeW, A, D, S);
	ambient += A;
	diffuse += D;
	spec += S;

	float4 color = xTexture.Sample(s, pin.TexCoord / 1000.0f);
	float4 litColor = color * (ambient + diffuse + spec);
	float dst = distance(xEyePosW, pin.PosW);

	litColor.rgb = lerp(litColor.rgb, float3(0, 0, 0), saturate(dst / 200000));
	litColor.a = xMaterial.Diffuse.a;


	return GroundFromSpace(pin.PosW, litColor);
}

technique11 LightTech
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_5_0, PS()));
	}
}