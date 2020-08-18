#pragma once

#ifndef NATIVEALPHASUBMODULE_H
#define NATIVEALPHASUBMODULE_H

#include "SE.Native.h"
#include "Particle.h"
#include "NativeSubmodule.h"

namespace Particles {
	class NativeAlphaModule final : NativeSubmodule {
	private:
		enum Transition { NONE, LERP, RANDOM_LERP };
		
		Transition transition = NONE;
		float end1, end2;
		float* startAlphasArr;

	public:
		NativeAlphaModule(NativeModule* parent);

		void onInitialize(int particleArrayLength) override;
		void onParticlesActivated(int* particleIndexArr, Particle* particlesArrPtr, int length) override;
		void onUpdate(float deltaTime, Particle* particleArrPtr, int length) override;
		bool isValid() override;

		void setNone();
		void setLerp(float end);
		void setRandomLerp(float min, float max);

		~NativeAlphaModule();
	};
}

#endif