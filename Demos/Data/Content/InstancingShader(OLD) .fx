//Ah almost forgot you'll need the shader too.

//_________________________________________________________________
//_______________________ InstancingShader ___________________________
//_________________________________________________________________


// the shader model well use for dx or gl
#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0
#define PS_SHADERMODEL ps_4_0
#endif



//______________________________________
// shader constants.
//______________________________________


//static const float PI = 3.14159;
//static const float PI2 = 6.28318;
//static const float EIGHT_PI = 25.13274;



//______________________________________
// shader variables.
// we set these up in game1.
//______________________________________


matrix World , View , Projection;



//______________________________________
// the shader textures and samplers.
// we set the texture to use thru game1
//______________________________________


Texture2D ParticleTexture;
sampler2D TexSampler = sampler_state
{
    Texture = <ParticleTexture>;
	//AddressU = Wrap;//AddressV = Wrap;//MinFilter = Anisotropic;//MagFilter = Anisotropic;//MipFilter = Point;
};




//______________________________________
// the shader structs well be defining.
// these match the vertex definitions in game1.
//______________________________________


struct VSInstanceInputSimple
{
    float3 InstancePosition : POSITION1; // the number used must match the vertex declaration.
    float4 InstanceColor : COLOR1;
};

struct VSVertexInputSimple
{
    float4 Position : POSITION0; 
    float2 TexCoord : TEXCOORD0;
};

struct VSOutputSimple
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 IdColor : COLOR0;
};




//______________________________________
// the vertex shaders.
//______________________________________

float4 hsl2rgb(float4 c)
{
    float3 rgb = clamp( abs(fmod(c.x*6.0+float3(0.0,4.0,2.0),6.0)-3.0)-1.0, 0.0, 1.0);
    float3 thing = c.z + c.y * (rgb-0.5)*(1.0-abs(2.0*c.z-1.0));
    return float4(thing, c.w);
}

VSOutputSimple VertexShader01(in VSVertexInputSimple vertexInput, VSInstanceInputSimple instanceInput)
{
    VSOutputSimple output;
    float4x4 vp = mul(View, Projection);
    float4x4 wvp = mul(World, vp);
    float4 posVert = mul(vertexInput.Position, wvp);
    float4 posInst = mul(instanceInput.InstancePosition.xyz, wvp);
    output.Position = posVert + posInst;
    output.TexCoord = vertexInput.TexCoord;
    output.IdColor = instanceInput.InstanceColor;
    return output;
}



//______________________________________
// the pixel shaders.
//______________________________________


float4 PixelShader01(VSOutputSimple input) : COLOR0
{
      float4 col = tex2D(TexSampler, input.TexCoord) * hsl2rgb(input.IdColor);
      return float4(col.rgb * input.IdColor.rgb, col.a);
}



//______________________________________
// the techniques.
// we set this from game1.
//______________________________________


technique ParticleInstancing
{
    pass
    {
        VertexShader = compile VS_SHADERMODEL
        VertexShader01();
        PixelShader = compile PS_SHADERMODEL
        PixelShader01();
    }
};