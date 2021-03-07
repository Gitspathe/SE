#include "Particle.h"
#include "ParticleMath.h"

const float Particles::ParticleMath::lerp(const float value1, const float value2, const float amount)
{
	return value1 + (value2 - value1) * amount;
}

const float Particles::ParticleMath::between(const float min, const float max, const float ratio)
{
	return (min + ((max - min) * ratio));
}

void Particles::ParticleMath::swap(float* x, float* y)
{
	float tmpX = *x;
	*x = *y;
    *y = tmpX;
}
