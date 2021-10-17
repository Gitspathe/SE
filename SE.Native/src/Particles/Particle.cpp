#include "ParticleMath.h"
#include "Particle.h"

namespace Particles {


    ParticleColor::ParticleColor()
    {
        ParticleColor(0, 0, 0, 0);
    }

    ParticleColor::ParticleColor(const float h, const float s, const float l, const float a)
    {
        packedValue = 0;

        setHueByte(h == 0 ? (uint8_t)0 : (uint8_t)ParticleMath::clamp((float)((h / 360.0f) * 255), (float)0.0f, (float)UINT8_MAX));
        setSaturationByte(s == 0 ? (uint8_t)0 : (uint8_t)ParticleMath::clamp((float)((s / 100.0f) * 255), (float)0.0f, (float)UINT8_MAX));
        setLightnessByte(l == 0 ? (uint8_t)0 : (uint8_t)ParticleMath::clamp((float)((l / 100.0f) * 255), (float)0.0f, (float)UINT8_MAX));
        setAlphaByte(a == 0 ? (uint8_t)0 : (uint8_t)ParticleMath::clamp((float)((a / 1.0f) * 255), (float)0.0f, (float)UINT8_MAX));
    }

    ParticleColor ParticleColor::Lerp(ParticleColor value1, ParticleColor value2, float amount)
    {
        amount = ParticleMath::clamp(amount, 0, 1);
        return ParticleColor(ParticleMath::lerpByte(value1.getHueByte(), value2.getHueByte(), amount),
            ParticleMath::lerpByte(value1.getSaturationByte(), value2.getSaturationByte(), amount),
            ParticleMath::lerpByte(value1.getLightnessByte(), value2.getLightnessByte(), amount),
            ParticleMath::lerpByte(value1.getAlphaByte(), value2.getAlphaByte(), amount));
    }
}