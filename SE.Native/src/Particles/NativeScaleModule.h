#pragma once

#ifndef NATIVESCALESUBMODULE_H
#define NATIVESCALESUBMODULE_H

#include "src/SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"
#include "src/Utility/Curve.h"

namespace Particles {

	namespace ScaleTransition {
		enum Transition { None, Lerp, Curve, RandomCurve };
	}

	class NativeScaleModule final : NativeSubmodule {
	private:
		ScaleTransition::Transition transition = ScaleTransition::None;
		int particlesLength;
		float start, end;
		Vector2* startScalesArr = nullptr;
		float* rand = nullptr;
		Curve* curve = nullptr;

		void regenerateRandom();
		bool isRandom();

	public:
		bool absoluteValue = false;
		NativeScaleModule(NativeModule* const parent);

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

		~NativeScaleModule() override;
	};


}

#endif