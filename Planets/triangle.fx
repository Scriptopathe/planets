matrix World;
matrix View;
matrix Projection;
float4 xCameraPosition;
texture2D xTexture;
texture2D xTextureRock;

SamplerState s
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

struct VS_IN
{
	float4 pos : POSITION;
	float4 col : COLOR;
	// float4 normal : NORMAL;
};

struct VS_OUTPUT
{
	float4 pos : SV_POSITION;
	float4 col : COLOR;
	float4 pos3D : POSITION1;
	float4 texPos : POSITION2;
	// float4 normal : NORMAL;
};

/* -----------------------------------------------------------------
* Opérations vectorielles
* ---------------------------------------------------------------*/
float DotProduct(float4 lightPos, float4 pos3D, float4 normal)
{
	float4 lightDir = normalize(pos3D - lightPos);
	return dot(lightDir, normal);
}

/* -----------------------------------------------------------------
* Vertex Shader
* ---------------------------------------------------------------*/
VS_OUTPUT VS(VS_IN input)
{
	VS_OUTPUT output = (VS_OUTPUT)0;
	output.texPos = input.pos/10.0f;
	output.pos = mul(input.pos, World);
	output.pos3D = output.pos;
	output.pos = mul(output.pos, View);
	output.pos = mul(output.pos, Projection);

	output.col = normalize(mul(input.col, World));
	
	//output.normal = input.normal;
	return output;
}
/* -----------------------------------------------------------------
* Pixel Shader
* ---------------------------------------------------------------*/
float4 PS(VS_OUTPUT input) : SV_Target
{
	float factor = 0.4 + saturate(DotProduct(float4(800, 0, 100, 1), input.pos3D, input.col));
	return factor * lerp(xTexture.Sample(s, input.texPos.xy),
						 xTextureRock.Sample(s, input.texPos.xy), pow(saturate(abs(input.col.z*1.5)), 8));
}

technique11 Render
{
	pass P0
	{
		// SetGeometryShader(0);
		SetVertexShader(CompileShader(vs_5_0, VS()));
		SetPixelShader(CompileShader(ps_5_0, PS()));
	}
}