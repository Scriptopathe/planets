// Buffer mis à jour à chaque dessin d'objet.
cbuffer cbPerObject
{
	float4x4 xWorld;
	float4x4 xWorldInvTranspose;
	float4x4 xWorldViewProj;
	Material xMaterial;
};

cbuffer cbPerFrame
{
	float3 xCameraPos;   // The camera's current position
	float xFar;
	float xNear;
	DirectionalLight xDirLight;
	PointLight xPointLight;
	float3 xEyePosW;
};