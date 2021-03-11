#pragma once

#ifndef NATIVESPEEDSUBMODULE_H
#define NATIVESPEEDSUBMODULE_H

#include "src/SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"
#include "src/Utility.h"

namespace Particles {

	namespace SpeedTransition {
		enum Transition { None, Lerp, Curve, RandomCurve };
	}

	class NativeSpeedModule final : NativeSubmodule {
	private:
		SpeedTransition::Transition transition = SpeedTransition::None;
		int particlesLength;
		float start, end;
		float* rand = nullptr;
		Curve* curve = nullptr;

		void regenerateRandom();
		bool isRandom();

	public:
		NativeSpeedModule(NativeModule* const parent);

		bool absoluteValue = false;

		void onInitialize(const int32_t particleArrayLength) override;
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) override;
		void onUpdate(float deltaTime, Particle* const particleArrPtr, const int32_t length) override;
		const bool isValid() override;

		bool getAbsoluteValue();
		void setAbsoluteValue(bool val);

		void setNone();
		void setLerp(float start, float end);
		void setCurve(Curve* const curve);
		void setRandomCurve(Curve* const curve);

		~NativeSpeedModule() override;
	};
}

#endif