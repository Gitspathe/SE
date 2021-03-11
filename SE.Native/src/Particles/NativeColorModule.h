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
		Vector4 end1, end2;
		Vector4* startColorsArr = nullptr;
		Vector4* rand = nullptr;
		Vector4* randEndColors = nullptr;
		Curve4* curve = nullptr;

		void regenerateRandom();
		bool isRandom();

	public:
		NativeColorModule(NativeModule* const parent);

		void onInitialize(const int32_t particleArrayLength) override;
		void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length) override;
		void onUpdate(float deltaTime, Particle* const particleArrPtr, const int32_t length) override;
		const bool isValid() override;

		void setNone();
		void setLerp(Vector4 end);
		void setRandomLerp(Vector4 min, Vector4 max);
		void setCurve(Curve4* const curve);

		~NativeColorModule() override;
	};
}

#endif