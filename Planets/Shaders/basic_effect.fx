matrix World;
matrix View;
matrix Projection;
float4 xColor;

struct VS_IN
{
	float4 pos : POSITION;
};

struct VS_OUTPUT
{
	float4 pos : SV_POSITION;
};


/* -----------------------------------------------------------------
* Vertex Shader
* ---------------------------------------------------------------*/
VS_OUTPUT VS(VS_IN input)
{
	VS_OUTPUT output = (VS_OUTPUT)0;
	output.pos = mul(input.pos, World);
	output.pos = mul(output.pos, View);
	output.pos = mul(output.pos, Projection);
	return output;
}
/* -----------------------------------------------------------------
* Pixel Shader
* ---------------------------------------------------------------*/
float4 PS(VS_OUTPUT input) : SV_Target
{
	return xColor;
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