#include "Particle.h"
#include "ParticleMath.h"

float Particles::ParticleMath::lerp(float value1, float value2, float amount)
{
	return value1 + (value2 - value1) * amount;
}

float Particles::ParticleMath::between(float min, float max, float ratio)
{
	return (min + ((max - min) * ratio));
}

void Particles::ParticleMath::swap(float* x, float* y)
{
	float tmpX = *x;
	*x = *y;
    *y = tmpX;
}
