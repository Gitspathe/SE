#pragma once

#ifndef NATIVELIGHTNESSSUBMODULE_H
#define NATIVELIGHTNESSSUBMODULE_H

#include "src/SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"
#include "src/Utility/Curve.h"

namespace Particles {

	namespace LightnessTransition {
		enum Transition { None, Lerp, RandomLerp, Curve };
	}

	class NativeLightnessModule final : NativeSubmodule {
	private:
		LightnessTransition::Transition transition = LightnessTransition::None;
		int particlesLength;
		float end1, end2;
		float* startLightnessArr = nullptr;
		float* rand = nullptr;
		float* randEndLightness = nullptr;
		Curve* curve = nullptr;

		void regenerateRandom();
		bool isRandom();

	public:
		NativeLightnessModule(NativeModule* const parent);

		void onInitialize(const int32_t particleArrayLength) override;
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) override;
		void onUpdate(float deltaTime, Particle* const particleArrPtr, const int32_t length) override;
		const bool isValid() override;

		void setNone();
		void setLerp(float end);
		void setRandomLerp(float min, float max);
		void setCurve(Curve* const curve);

		~NativeLightnessModule() override;
	};


}

#endif