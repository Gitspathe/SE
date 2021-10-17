#pragma once

#ifndef NATIVECOLORSUBMODULE_H
#define NATIVECOLORSUBMODULE_H

#include "src/SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"
#include "src/Utility.h"

namespace Particles {

	namespace ColorTransition {
		enum Transition { None, Lerp, RandomLerp, Curve };
	}

	class NativeColorModule final : NativeSubmodule {
	private:
		ColorTransition::Transition transition = ColorTransition::None;
		int particlesLength;
		ParticleColor end1, end2;
		ParticleColor* startColorsArr = nullptr;
		ParticleColor* randEndColors = nullptr;
		Curve4* curve = nullptr;

		void regenerateRandom();
		bool isRandom();

	public:
		NativeColorModule();

		void onInitialize(const int32_t particleArrayLength) override;
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) override;
		void onUpdate(float deltaTime, Particle* const particleArrPtr, const int32_t length) override;
		const bool isValid() override;

		void setNone();
		void setLerp(ParticleColor end);
		void setRandomLerp(ParticleColor min, ParticleColor max);
		void setCurve(Curve4* const curve);

		~NativeColorModule() override;
	};
}

#endif