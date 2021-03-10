#pragma once

#ifndef NATIVEALPHASUBMODULE_H
#define NATIVEALPHASUBMODULE_H

#include "src/SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"
#include "src/Utility/Curve.h"

namespace Particles {

	namespace AlphaTransition {
		enum Transition { None, Lerp, RandomLerp, Curve };
	}

	class NativeAlphaModule final : NativeSubmodule {
	private:		
		AlphaTransition::Transition transition = AlphaTransition::None;
		int particlesLength;
		float end1, end2;
		float* startAlphasArr = nullptr;
		float* rand = nullptr;
        float* randEndAlphas = nullptr;
		Curve* curve = nullptr;

		void regenerateRandom();
		bool isRandom();

	public:
		NativeAlphaModule(NativeModule* parent);

		void onInitialize(const int32_t particleArrayLength) override;
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) override;
		void onUpdate(float deltaTime, Particle* const __restrict particleArrPtr, const int32_t length) override;
		const bool isValid() override;

		void setNone();
		void setLerp(float end);
		void setRandomLerp(float min, float max);
		void setCurve(Curve* const curve);

		~NativeAlphaModule() override;
	};
}

#endif