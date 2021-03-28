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

matrix World;

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
    float2 TexCoordOffset : TEXCOORD1;
	float2 InstanceScale : POSITION2;
    float InstanceRotation : POSITION3;
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

float4x4 m_scale(float4x4 m, float2 v)
{
    float x = v.x, y = v.y;

    m[0][0] *= x; m[1][0] *= y;
    m[0][1] *= x; m[1][1] *= y;
    m[0][2] *= x; m[1][2] *= y;
    m[0][3] *= x; m[1][3] *= y;

    return m;
}

float4 hsl2rgb(float4 c)
{
    float3 rgb = clamp( abs(fmod(c.x*6.0+float3(0.0,4.0,2.0),6.0)-3.0)-1.0, 0.0, 1.0);
    float3 thing = c.z + c.y * (rgb-0.5)*(1.0-abs(2.0*c.z-1.0));
    return float4(thing, c.w);
}

VSOutputSimple VertexShader01(in VSVertexInputSimple vertexInput, VSInstanceInputSimple instanceInput)
{
    VSOutputSimple output;

    float instanceRotation = instanceInput.InstanceRotation;
    float4x4 rotationAroundZ = {
        cos(instanceRotation), -sin(instanceRotation), 0.0f, 0.0f,
        sin(instanceRotation), cos(instanceRotation), 0.0f, 0.0f,
        0.0f, 0.0f, 1.0f, 0.0f,
        0.0f, 0.0f, 0.0f, 1.0f
    };
	
    // Apply rotation.
    float4x4 rotated = mul(rotationAroundZ, World);

    // Apply scale.
    float4x4 scaledWvp = m_scale(rotated, instanceInput.InstanceScale);

    // Apply the transformation.
    float4 posVert = mul(vertexInput.Position, scaledWvp);
    float4 posInst = mul(instanceInput.InstancePosition, World);

    output.Position = posVert + posInst;
    output.TexCoord = vertexInput.TexCoord + instanceInput.TexCoordOffset;
    output.IdColor = instanceInput.InstanceColor;
    return output;
}



//______________________________________
// the pixel shaders.
//______________________________________


float4 PixelShader01(VSOutputSimple input) : COLOR0
{
    return tex2D(TexSampler, input.TexCoord) * hsl2rgb(input.IdColor);
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