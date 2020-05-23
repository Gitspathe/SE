#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;

sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 hsl2rgb(float4 c)
{
    float3 rgb = clamp( abs(fmod(c.x*6.0+float3(0.0,4.0,2.0),6.0)-3.0)-1.0, 0.0, 1.0);
    float3 thing = c.z + c.y * (rgb-0.5)*(1.0-abs(2.0*c.z-1.0));
    return float4(thing, c.w);
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	return tex2D(SpriteTextureSampler,input.TextureCoordinates) * hsl2rgb(input.Color);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};