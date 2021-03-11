#pragma once

#ifndef NATIVESPRITEROTATIONSUBMODULE_H
#define NATIVESPRITEROTATIONSUBMODULE_H

#include "src/SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"
#include "src/Utility.h"

namespace Particles {

	namespace SpriteRotationTransition {
		enum Transition { None, Constant, Lerp, Curve, RandomConstant, RandomCurve };
	}

	class NativeSpriteRotationModule final : NativeSubmodule {
	private:
		SpriteRotationTransition::Transition transition = SpriteRotationTransition::None;
		int particlesLength;
		float start, end;
		float* rand = nullptr;
		Curve* curve = nullptr;

		void regenerateRandom();
		bool isRandom();

	public:
		NativeSpriteRotationModule(NativeModule* const parent);

		void onInitialize(const int32_t particleArrayLength) override;
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) override;
		void onUpdate(float deltaTime, Particle* const particleArrPtr, const int32_t length) override;
		const bool isValid() override;

		void setNone();
		void setConstant(float val);
		void setLerp(float start, float end);
		void setCurve(Curve* const curve);
		void setRandomConstant(float min, float max);
		void setRandomCurve(Curve* const curve);

		~NativeSpriteRotationModule() override;
	};
}

#endif