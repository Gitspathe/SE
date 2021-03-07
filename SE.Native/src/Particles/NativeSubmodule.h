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
		NativeSubmodule(NativeModule* parent);

		virtual void onInitialize(int32_t particleArrayLength);
		virtual void onParticlesActivated(int32_t* particleIndexArr, Particle* particlesArrPtr, const int32_t length);
		virtual void onUpdate(float deltaTime, Particle* __restrict particleArrPtr, const int32_t length);
		virtual bool isValid();

		virtual ~NativeSubmodule();
	};
}

#endif