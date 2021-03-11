#pragma once

#ifndef NATIVESATURATIONSUBMODULE_H
#define NATIVESATURATIONSUBMODULE_H

#include "src/SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"
#include "src/Utility.h"

namespace Particles {

	namespace SaturationTransition {
		enum Transition { None, Lerp, RandomLerp, Curve };
	}

	class NativeSaturationModule final : NativeSubmodule {
	private:
		SaturationTransition::Transition transition = SaturationTransition::None;
		int particlesLength;
		float end1, end2;
		float* startSaturationArr = nullptr;
		float* rand = nullptr;
		float* randEndSaturation = nullptr;
		Curve* curve = nullptr;

		void regenerateRandom();
		bool isRandom();

	public:
		NativeSaturationModule(NativeModule* const parent);

		void onInitialize(const int32_t particleArrayLength) override;
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) override;
		void onUpdate(float deltaTime, Particle* const particleArrPtr, const int32_t length) override;
		const bool isValid() override;

		void setNone();
		void setLerp(float end);
		void setRandomLerp(float min, float max);
		void setCurve(Curve* const curve);

		~NativeSaturationModule() override;
	};
}

#endif