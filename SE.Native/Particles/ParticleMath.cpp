#include "Particle.h"
#include "ParticleMath.h"

float Particles::ParticleMath::Lerp(float value1, float value2, float amount)
{
	return value1 + (value2 - value1) * amount;
}
