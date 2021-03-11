#pragma once

#ifndef NATIVESUBMODULE_H
#define NATIVESUBMODULE_H

#include "Particle.h"

namespace Particles {
	class NativeModule;
	class NativeSubmodule {
	protected:
		NativeModule* parent;
		bool isInitialized = false;

	public:
		NativeSubmodule(NativeModule* const parent);

		virtual void onInitialize(const int32_t particleArrayLength);
		virtual void onParticlesActivated(const int32_t* const particleIndexArr, Particle* const particlesArrPtr, const int32_t length);
		virtual void onUpdate(const float deltaTime, Particle* const __restrict particleArrPtr, const int32_t length);
		virtual const bool isValid();

		virtual ~NativeSubmodule();
	};
}

#endif