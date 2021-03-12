#pragma once

#ifndef NATIVEHUESUBMODULE_H
#define NATIVEHUESUBMODULE_H

#include "src/SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"
#include "src/Utility/Curve.h"

namespace Particles {

	namespace HueTransition {
		enum Transition { None, Lerp, RandomLerp, Curve };
	}

	class NativeHueModule final : NativeSubmodule {
	private:
		HueTransition::Transition transition = HueTransition::None;
		int particlesLength;
		float end1, end2;
		float* startHuesArr = nullptr;
		float* randEndHues = nullptr;
		Curve* curve = nullptr;

		void regenerateRandom();
		bool isRandom();

	public:
		NativeHueModule(NativeModule* const parent);

		void onInitialize(const int32_t particleArrayLength) override;
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) override;
		void onUpdate(float deltaTime, Particle* const particleArrPtr, const int32_t length) override;
		const bool isValid() override;

		void setNone();
		void setLerp(float end);
		void setRandomLerp(float min, float max);
		void setCurve(Curve* const curve);

		~NativeHueModule() override;
	};


}

#endif