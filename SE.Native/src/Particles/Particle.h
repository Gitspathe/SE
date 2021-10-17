#pragma once

#ifndef PARTICLE_H
#define PARTICLE_H

#include "src/Utility.h"
#include "src/Particles/ParticleMath.h"

using namespace Utility;

namespace Particles
{
	// TODO: Complete ParticleColor struct on native side.
	struct ParticleColor {
	public:
		inline float getAlpha()
		{
			return getAlphaByte() / 255.0f;
		}

		inline void setAlpha(const float val)
		{
			setAlphaByte(ParticleMath::clamp((uint8_t)(val * 255), (uint8_t)0, (uint8_t)UINT8_MAX));
		}

		inline float getLightness()
		{
			return (getLightnessByte() / 255.0f) * 100.0f;
		}

		inline void setLightness(const float val)
		{
			setLightnessByte(ParticleMath::clamp((uint8_t)((val / 100.0f) * 255.0f), (uint8_t)0, (uint8_t)UINT8_MAX));
		}

		inline float getSaturation()
		{
			return (getSaturationByte() / 255.0f) * 100.0f;
		}

		inline void setSaturation(const float val)
		{
			setSaturationByte(ParticleMath::clamp((uint8_t)((val / 100.0f) * 255.0f), (uint8_t)0, (uint8_t)UINT8_MAX));
		}

		inline float getHue()
		{
			return (getHueByte() / 255.0f) * 360.0f;
		}

		inline void setHue(const float val)
		{
			setHueByte(ParticleMath::clamp((uint8_t)((val / 360.0f) * 255.0f), (uint8_t)0, (uint8_t)UINT8_MAX));
		}

		inline const uint8_t getAlphaByte()
		{
			return (uint8_t)(packedValue >> 24);
		}


		inline const uint8_t getLightnessByte()
		{
			return (uint8_t)(packedValue >> 16);
		}


		inline const uint8_t getSaturationByte()
		{
			return (uint8_t)(packedValue >> 8);
		}


		inline const uint8_t getHueByte()
		{
			return (uint8_t)(packedValue);
		}

		ParticleColor();
		ParticleColor(const float h, const float s, const float l, const float a);

		static ParticleColor Lerp(const ParticleColor value1, const ParticleColor value2, float amount);

	private:
		uint32_t packedValue;

		inline void setAlphaByte(const uint8_t val)
		{
			packedValue = (packedValue & 0x00ffffff) | ((uint32_t)val << 24);
		}

		inline void setLightnessByte(const uint8_t val)
		{
			packedValue = (packedValue & 0xff00ffff) | ((uint32_t)val << 16);
		}

		inline void setSaturationByte(const uint8_t val)
		{
			packedValue = (packedValue & 0xffff00ff) | ((uint32_t)val << 8);
		}


		inline void setHueByte(const uint8_t val)
		{
			packedValue = (packedValue & 0xffffff00) | (val);
		}
	};

	struct Particle {
	public:
		Vector2 position;
		Vector2 scale;
		float spriteRotation;
		ParticleColor color;

		int32_t id;
		Vector2 direction;
		float mass;
		float speed;
		float initialLife;
		float timeAlive;
		float layerDepth;
		Int4 sourceRectangle;
	};
}

#endif