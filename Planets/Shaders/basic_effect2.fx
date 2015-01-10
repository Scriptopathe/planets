#include "Shaders\\core.fx"
#include "Shaders\\noise.fx"
#include "Shaders\\scattering.fx"
/* -----------------------------------------------------------------
* Varibales / buffers.
* ---------------------------------------------------------------*/
// Buffer mis à jour toutes les frames.

texture2D xTexture;
texture2D xTexture2;
texture2D xTexture3;



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
	float4 NormalP : PNORMAL;
	float2 TexCoord  : TEXTURE;
	float TexId : TEXTURE_ID;
	float Altitude : ALTITUDE;
};

struct VertexOut
{
	float4 PosH    : SV_POSITION;
	float3 PosW    : POSITION;
	float3 NormalW : NORMAL;
	float4 NormalP : PNORMAL;
	float2 TexCoord : TEXTURE;
	float TexId : TEXTURE_ID;
	float Altitude : ALTITUDE;
};

VertexOut VS(VertexIn vin)
{
	VertexOut vout;

	vout.PosW = mul(vin.PosL, xWorld).xyz;
	vout.NormalW = vin.NormalL.xyz;//mul(vin.NormalL, (float3x3)xWorldInvTranspose);
	vout.NormalP = vin.NormalP;
	vout.PosH = mul(vin.PosL, xWorldViewProj);
	vout.TexCoord = vin.TexCoord;
	vout.TexId = vin.TexId;
	vout.Altitude = vin.Altitude;
	return vout;
}



float4 PS(VertexOut pin) : SV_Target
{
	// return 0.5 + float4(0, pin.NormalW.y, pin.NormalW.z, 1)/2;
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

	float4 grass = xTexture.Sample(s, pin.TexCoord * 1000.0f);
	float4 rock = xTexture2.Sample(s, pin.TexCoord * 1000.0f);
	float4 sand = xTexture3.Sample(s, pin.TexCoord * 1000.0f);
	float4 color = lerp(grass, rock, saturate(pow(abs(-pin.NormalP.y), 1)));
	color = lerp(color, sand, pow(saturate(pin.TexId*1.9), 4));
	color = lerp(color, float4(0.8, 0.7, 0.7, 1), saturate(pin.Altitude*10));
	color = lerp(float4(0.8, 0.7, 0, 1), color, saturate(pin.Altitude * 200));
	float4 litColor = color * (ambient + diffuse + spec);

	float dst = distance(xEyePosW, pin.PosW);

	litColor.a = xMaterial.Diffuse.a;
	return GroundFromSpace(pin.PosW, litColor);
	/*if (fCameraHeight > fOuterRadius)
		return GroundFromSpace(pin.PosW, litColor);
	else
		return GroundFromAtmosphere(pin.PosW, litColor);*/
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