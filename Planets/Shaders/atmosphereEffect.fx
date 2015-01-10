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
	float3 PosW    : TEXCOORD0;
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
	float4 litColor = float4(1, 1, 1, 1);
	if (fCameraHeight > fOuterRadius)
		return SkyFromSpace(pin.PosW, litColor);
	else
		return SkyFromAtmosphere(pin.PosW, litColor);
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